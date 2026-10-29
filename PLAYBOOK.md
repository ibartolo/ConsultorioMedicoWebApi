# Playbook de desarrollo — ConsultorioMedico (MVC + WebApi)

> Guía consolidada para **agregar módulos nuevos** (Cita, Consulta, Paquete, Tratamiento, Usuario, etc.).
> Patrón validado con el módulo **Paciente**.

---

## 1. Contexto / solución

- **Solución**: `DESI.ConsultorioMedico.sln` (raíz del repo).
- **3 proyectos** (`.NET Framework 4.8`):
  - `ConsultorioMedicoEntities` → entidades, DTOs, envoltura de respuesta, validadores.
  - `ConsultorioMedicoWebApi` → ASP.NET Web API (OWIN + OAuth), acceso a datos (ADO.NET + SPs).
  - `ConsultorioMedicoMVC` → ASP.NET MVC 5, frontend (Bootstrap 5 + jQuery).
- **Referencia de arquitectura**: `C:\Git\ServiceDeskDESI\ServiceDeskDESI\ServiceDeskDESIMVC`.
- **Base de datos**: SQL Server productiva (`DB_9C7990_Consultorio`, Smarterasp). **NO eliminar nada**.
- **Migración**: la app WinForms (NHibernate) se migra "tal como funciona" a MVC + WebApi.

---

## 2. Arquitectura de capas (flujo completo)

```
Vista (jQuery Validate + hidden inputs + serialize)
  → Controller MVC      (SOLO delega; model binding)
  → Servicio MVC        (valida + lógica "si es nuevo")
  → HttpClientConnection (HTTP a la WebApi)
  → Controller WebApi   (delega)
  → Servicio WebApi     (valida de nuevo + "si es nuevo" fallback)
  → DbWrapper           (SPs / ADO.NET)
  → Base de datos
```

---

## 3. Reglas de acceso a datos (NHibernate → SPs)

- **NHibernate muere → todo por Stored Procedures**.
- Si la WinForms usaba un **SP existente** → **reutilizar tal cual**.
- Si usaba **NHibernate** (SaveOrUpdate/CreateCriteria/Get) o **SQL inline** → crear **SP nuevo** con prefijo `sp_nh_` + nombre del método NHibernate.
  - Ej.: `sp_nh_GetPacienteById`, `sp_nh_SavePaciente`, `sp_nh_AutenticacionUsuario`.
- Los SPs van en archivos `sql/*.sql`, **solo `CREATE`** con protección `IF OBJECT_ID` (nunca `DROP`/`ALTER` en BD productiva).
- Listados: reutilizar SPs existentes (ej. `GetAllPacienteByEstatus` que ya calcula `NombreCompleto` y `Edad` al vuelo).
- El upsert NO escribe auditoría (mismo comportamiento que NHibernate en la app actual).

---

## 4. Checklist para agregar un módulo

### 4.1 Entities (`ConsultorioMedicoEntities`)
- `Catalogos/<Entidad>.cs` (hereda `BaseObject` si tiene columnas de auditoría; si no, POCO plano).
- `Catalogos/<Entidad>DTO.cs` (hereda de la entidad, agrega campos de display/join como `NombreCompleto`).
- `Catalogos/<Entidad>Validator.cs` → **validador estático compartido** (ver §6).
- Registrar el `.cs` en el `.csproj` (`<Compile Include="...">`).

### 4.2 SQL (`sql/`)
- Crear `sql/sp_nh_<Entidad>.sql` con los SPs nuevos (solo `CREATE`, protección `IF OBJECT_ID`).

### 4.3 WebApi (`ConsultorioMedicoWebApi`)
- `DAL/DbWrapper.<Entidad>.cs` (partial): `ObtenerX`, `ObtenerXPorId`, `GuardarOActualizarX` (upsert, `ExecuteScalar` devuelve id).
- `Services/<Entidad>Service.cs`: `new DbWrapper()`; valida + "si es nuevo" + `Simulador.Activo`.
- `Controllers/<Entidad>Controller.cs`: `[Authorize] [RoutePrefix("api/<Entidad>")]` con `List`, `{id:long}`, `Guardar`.
- Registrar los `.cs` en el `.csproj`.

### 4.4 MVC (`ConsultorioMedicoMVC`)
- `DAL/HttpClientConnection.<Entidad>.cs` (partial): métodos async → `RequestAsync` con `token.Token.access_token`.
- `Services/<Entidad>Service.cs`: ctor `(HttpClientConnection)`; valida + "si es nuevo" + passthrough.
- `Controllers/<Entidad>Controller.cs`: región `Views` (`X(long id = 0)`) + región `Data Access` (`Task<string>` → `JsonConvert.SerializeObject`).
- Registrar los `.cs` en el `.csproj`.

### 4.5 Vistas (`ConsultorioMedicoMVC/Views/<Entidad>/`)
- `Index.cshtml` → DataTable con listado (AJAX `ConsultarTodosX`).
- `Nuevo.cshtml` → formulario (ver §5).
- Registrar los `.cshtml` en el `.csproj` como `<Content>`.

---

## 5. Patrón de formulario (jQuery Validate)

- Bootstrap 5 (CDN) + Font Awesome + SweetAlert2 + DataTables. `jquery.validate` 1.19.5 (cdnjs) ya cargado en `_Layout.cshtml`.
- Campos visibles con atributo `name` (jQuery Validate usa `name`).
- **Todos los datos no visibles** (Id, Estatus, fechas, auditoría) en `<input type="hidden" name="...">`:
  - Fechas `DateTime` en hidden: `value="@(Model.X.HasValue ? Model.X.Value.ToString("yyyy-MM-dd HH:mm:ss") : "")"`.
- Botón: `<input type="button" value="Guardar" onclick="guardarX()" />` (NO `type="submit"`).
- El `<form>` solo es contenedor para validación (no se envía nativo).

```js
$.validator.addMethod("telefono10", function (value, element) {
    return this.optional(element) || /^\d{10}$/.test(value);   // SIEMPRE this.optional(element), no value
}, "El teléfono debe tener exactamente 10 dígitos.");

$('#frmX').validate({ rules: {...}, messages: {...}, errorElement:'div', errorClass:'invalid-feedback',
    highlight: el => $(el).addClass('is-invalid'), unhighlight: el => $(el).removeClass('is-invalid') });

function guardarX() {
    if (!$('#frmX').valid()) { return; }        // si no cumple, salir
    $.ajax({
        url: guardarUrl,
        type: 'POST',
        data: $('#frmX').serialize(),            // form data (NO JSON.stringify)
        dataType: 'json',
        success: function (response) {
            var r = (typeof response === 'string') ? JSON.parse(response) : response;
            if (r.IsSuccess) { /* Swal éxito + redirect */ } else { /* Swal error r.Message */ }
        },
        error: function () { /* Swal error conexión */ }
    });
}
```

- Enlaces de edición: ruta `/Controller/Accion/{id}` (no querystring `?id=`).

---

## 6. Validación en capas

- **Validador compartido en Entities** (referenciado por MVC y WebApi), con las MISMAS reglas que jQuery Validate:

```csharp
public static class XValidator {
    public static string Validar(X e) {
        if (string.IsNullOrWhiteSpace(e.Nombre)) return "El nombre es requerido.";
        if (!Regex.IsMatch(e.TelefonoCel, @"^\d{10}$")) return "El teléfono debe tener exactamente 10 dígitos.";
        if (!Regex.IsMatch(e.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return "Ingrese un formato de correo válido.";
        return null; // sin errores
    }
}
```

- **Servicio MVC** → `var error = XValidator.Validar(e); if (error != null) return ModelResponse{IsSuccess=false, Message=error};` + lógica "si es nuevo".
- **Servicio WebApi** → misma validación (garantiza integridad para la futura **app móvil**) + "si es nuevo" fallback.

Lógica "si es nuevo" (en servicios, NO en controller):
```csharp
if (e.Id == 0) {
    e.Estatus = true;
    if (e.FechaAlta == null) e.FechaAlta = DateTime.Now;
}
```

---

## 7. Gotchas obligatorios

1. **BOM UTF-8**: TODO archivo con acentos/ñ debe guardarse con BOM UTF-8. El `write` tool NO agrega BOM → re-guardar con:
   ```powershell
   $b = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
   [System.IO.File]::WriteAllText($path, $b, (New-Object System.Text.UTF8Encoding $true))
   ```
2. **Razor = C# 5**: en `.cshtml` NO usar `?.` ni interpolación `$""` (el compilador de vistas de MVC 5 no los soporta). En `.cs` (C# 7.3 Roslyn) sí se puede.
3. **MVC model binding = form data**, no JSON body. Usar `serialize()` + `dataType:'json'`. NO `contentType:'application/json'` + `JSON.stringify` (MVC 5 no bindea JSON a parámetros complejos).
4. **OAuth password grant**: campos `username`/`password` en **minúscula**. `ValidateClientAuthentication` con `context.Validated()`.
5. **Binding redirects** en `Web.config`:
   - Owin → `4.2.3.0`; Owin.Security.OAuth → `3.0.1.0`.
   - `System.Web.WebPages/Helpers/Razor` → `3.0.0.0` (NO `3.3.0.0`; el AssemblyVersion real es 3.0.0.0).
   - Newtonsoft.Json → `13.0.0.0`.
6. **`<globalization>`** en `system.web` (ambos Web.config): `requestEncoding/responseEncoding/fileEncoding="utf-8"` + `culture="es-MX"`.
7. **`machineKey` fija** compartida entre MVC y WebApi (para token OAuth + cookie FormsAuth).

---

## 8. Modo simulación (sin BD)

> ⚠️ **La simulación fue eliminada** (2026-09-02). La app va directo a la BD real.
> Si se necesita de nuevo, recuperar `Services/Simulador.cs` y la llave `SimularDatos` desde el historial de git.

---

## 9. Comandos de verificación

```powershell
# Compilar toda la solución
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" "DESI.ConsultorioMedico.sln" /t:Build /p:Configuration=Debug

# Precompilar las vistas (detecta errores Razor)
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -p "ConsultorioMedicoMVC" -v / "C:\Users\Wayssen\AppData\Local\Temp\opencode\mvc_check"
```

Ambos deben terminar con **exit 0**.

---

## 10. Convenciones rápidas

- `ModelResponse<T>`: envoltura de respuesta (`IsSuccess`, `Message`, `Response`). HTTP 200 siempre; errores en `IsSuccess=false` + `Message` en español.
- JSON: WebApi serializa camelCase; MVC serializa PascalCase. Newtonsoft deserializa case-insensitive → el flujo funciona.
- `Id` = `int` (la BD usa `int IDENTITY`, no `long`).
- Auditoría BD: `CreatedBY`/`CreatedDT`/`UpdatedBY`/`UpdatedDT` (en esta BD `CreatedDT`/`UpdatedDT` son `nvarchar(25)`, no datetime — los SPs no las tocan).

---

## 11. Alcance actual y cambios recientes (2026-10)

### Módulos
- Paciente, Cita, Consulta, Tratamiento, Paquete, Usuario, Anexo, Historial/Expediente, Receta, Datos Institución, Datos Fiscales, Configuración, Dashboard.
- Menú: **Catálogos** agrupa Tratamientos/Paquetes. **Historial**, **Anexos** y **Reportes** están ocultos del menú (las pantallas siguen existiendo por URL directa).

### Anexos
- `dbo.Anexo`: `PacienteID` obligatorio, `ConsultaID` opcional. No hay `CitaID` ni `PaqueteID`.
- Formatos: PDF/PNG/JPG/JPEG. PDF <=5 MB; imágenes <=2 MB.
- Archivo físico: WebApi `Attachment/{pacienteId}/[{consultaId}/]{guid}.{ext}`.
- `Nombre` = nombre original (150); `Ruta` = ruta parcial `/Attachment/...`; `Comentarios` máx. 1500.
- Subida desde Pacientes (modal Anexos) y Expediente → Consultas (clip). Descarga por proxy MVC.

### Receta
- Una por consulta; `DescripcionReceta` es texto libre.
- Se abre en modal (`Consulta/RecetaParcial`); `Consulta/Receta/{id}` queda como respaldo.
- Membrete desde `DatosInstitucion`: logo, nombre, dirección, teléfonos, correo y médico (nombre, cédula, especialidad).
- Impresión aislada en ventana con solo la receta; tamaño carta.

### Configuración / cookies
- `tema` (claro/oscuro) y `recordarUsuario` (solo usuario), 1 año, HttpOnly.
- La cookie `tema` se crea en login y se renueva al cambiarla; `Home.CambiarTema` es acción pública.
- `AuthenticationFilter` tiene allow-list `PublicActions`; el manual es privado y términos/privacidad son públicos.

### Dashboard
- KPIs del mes: pacientes nuevos (`FechaAlta`/`CreatedDT`), consultas (`Consulta.FechaConsulta`), tratamientos (citas con `TratamientoID`).
- Endpoint: `api/Dashboard/Indicadores` → MVC `/Dashboard/Indicadores`.
- Consulta inline de solo lectura en `DbWrapper.Dashboard.cs`.

### Calendarios
- Citas y Consultas: `slotDuration`/`slotLabelInterval` 30 min, `slotMinTime` 08:00, `slotMaxTime` 20:00, `scrollTime` 08:00, `allDaySlot: false`.
- Aplica a vistas semana/día; la vista mensual sigue mensual.

### DataTables
- Idioma local en `Content/datatables-es-ES.json` (evita CORS del CDN).

### Documentos
- Manual `/Home/Manual` (autenticado), Términos `/Home/Terminos` y Privacidad `/Home/Privacidad` (públicos), con layout `_LayoutPublic`.
- Footer autenticado: ©, Manual, Términos, Privacidad, By DESI.
