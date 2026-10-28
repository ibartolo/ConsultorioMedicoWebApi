# Design: Paciente — CRUD en modales + baja lógica

## Contexto

Migrar alta/edición de Paciente de la página full-page (`Views/Paciente/Nuevo.cshtml`) a un **modal ancho y largo** reutilizable (`modal-xl`), añadir **baja lógica** (Eliminar → `Estatus=0`) hoy inexistente, y refrescar **solo la fila afectada** en DataTables 1.13.6 (sin `ajax.reload()` ni `window.location`). Confirmaciones con SweetAlert2. No se toca Expediente/Cita/Consulta; la ruta `Paciente/Nuevo/{id}` se conserva como wrapper.

**Fuente**: `proposal.md` + `specs/paciente/spec.md`. Patrón de baja replicado de `sp_nh_DeleteCita` (WebApi/MVC Cita `EliminarCita`).

### Reconciliaciones con el código real (importante)

1. **`GuardarOActualizarPaciente` devuelve `ModelResponse<Paciente>`** (entidad con `Id` ya seteado en `DbWrapper.Paciente.GuardarOActualizarPaciente`), **no** `ModelResponse<int>` como asume el proposal. El JS lee `result.Response.Id`.
2. **`api/Paciente/{id}` (sp_nh_GetPacienteById) NO devuelve `NombreCompleto`** ni campos de auditoría; devuelve `Paciente` plano. `NombreCompleto` solo lo calcula `GetAllPacienteByEstatus`. Se resuelve componiéndolo en el servicio MVC (D3).

---

## Decisiones de arquitectura

### D1 — Extraer `_Formulario.cshtml` como partial

**Choice**: `_Formulario.cshtml` (`@model ConsultorioMedicoEntities.Catalogos.Paciente`) contiene SOLO el `<form id="pacienteForm" autocomplete="off" novalidate>` con los 8 hidden (Id, Estatus, FechaAlta, CreatedBY, CreatedDT, UpdatedBY, UpdatedDT, Edad) + campos visibles (Nombre, ApellidoPaterno, ApellidoMaterno, Sexo, FechaNacimiento `type="date"`, TelefonoCel, Email, Comentarios). **Sin botones** (Cancelar/Guardar) y **sin `<script>`**. `Nuevo.cshtml` queda como wrapper que renderiza `@Html.Partial("_Formulario", Model)`.

**Alternativas**: (a) duplicar el form en modal y página; (b) incluir botones en el partial; (c) incluir `<script>` de validación en el partial.

**Rationale**: los campos son idénticos en ambos flujos (DRY). Los botones difieren (full-page navega, modal cierra), por eso van fuera. **El partial no puede llevar `<script>`**: `_Layout.cshtml` carga jQuery/validate DESPUÉS de `@RenderBody()`, así que cualquier script inline del partial se ejecutaría antes de cargar jQuery y fallaría. Toda la lógica JS vive en `@section scripts` de cada página.

### D2 — Modal `modal-xl` con altura amplia

**Choice**: en `Views/Paciente/Index.cshtml`:
```html
<div class="modal fade" id="modalPaciente" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-xl modal-dialog-scrollable">
    <div class="modal-content">
      <div class="modal-header">…<button class="btn-close" data-bs-dismiss="modal"></button></div>
      <div class="modal-body">@Html.Partial("_Formulario", new ConsultorioMedicoEntities.Catalogos.Paciente())</div>
      <div class="modal-footer">
        <button class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
        <input type="button" value="Guardar" class="btn btn-primary" onclick="guardarPacienteModal()" />
      </div>
    </div>
  </div>
</div>
```
El partial se renderiza con `new Paciente()` (vacío); el JS lo puebla. Altura amplia garantizada con `modal-dialog-scrollable` (Bootstrap 5.3 nativo) **+** regla en `Content/site.css`: `.modal-xl .modal-body { max-height: 70vh; overflow-y: auto; }`.

**Alternativas**: solo `modal-dialog-scrollable` (scroll nativo pero sin altura fija), o CSS `min-height` rígido, o inyectar el partial por AJAX (carga dinámica del HTML).

**Rationale**: `modal-xl` da ancho; `scrollable`+`max-height:70vh` da la "altura amplia" pedida y scroll interno cómodo sin desbordar la ventana. Renderizar el partial servidor-side evita un fetch extra de HTML y mantiene la inicialización de jQuery Validate determinista.

### D3 — Carga para editar: `ConsultarPacientePorId` (JSON) + formato de fecha

**Choice**: nueva acción MVC `ConsultarPacientePorId(long id)` que devuelve `ModelResponse<PacienteDTO>` (PascalCase, vía `JsonConvert.SerializeObject`). El servicio MVC compone `NombreCompleto` (el endpoint `api/Paciente/{id}` no lo devuelve):

```csharp
// MVC Services/PacienteService.cs
public async Task<ModelResponse<PacienteDTO>> ConsultarPacientePorId(long id)
{
    var r = await _httpClient.ObtenerPacientePorId(id); // ModelResponse<Paciente>
    if (!r.IsSuccess || r.Response == null)
        return new ModelResponse<PacienteDTO> { IsSuccess = false, Message = r.Message ?? "Paciente no encontrado." };
    var p = r.Response;
    var dto = new PacienteDTO { /* copiar todos los campos de p */ };
    dto.NombreCompleto = ($"{p.Nombre} {p.ApellidoPaterno} {p.ApellidoMaterno}").Trim();
    return new ModelResponse<PacienteDTO> { IsSuccess = true, Response = dto, Message = r.Message };
}
```
El JS `abrirModalPaciente(id, tr)`: `id>0` → `$.get(consultarUrl, {id}, …)` → `poblarForm(r.Response)`; **fallback** a `table.row(tr).data()` si el GET falla o `!IsSuccess`; `id===0` → `resetForm()` + `Id=0`. Formato de fechas: `FechaNacimiento` → `yyyy-MM-dd` para `input type="date"` usando `iso.substring(0,10)` (evita desfase de zona horaria de `new Date(iso)`); `FechaAlta/CreatedDT/UpdatedDT` (hidden) → `yyyy-MM-dd HH:mm:ss` (igual que `ToString("yyyy-MM-dd HH:mm:ss")` actual).

**Alternativas**: (a) devolver `Paciente` plano y componer `NombreCompleto` en JS (precedente `expediente-clinico-historial` D3); (b) usar `row().data()` como fuente única (proposal opción a).

**Rationale**: el spec exige que la acción devuelva datos frescos "incluyendo `NombreCompleto` y `Edad`". `Edad` ya viene en `Paciente`; `NombreCompleto` se compone en el servicio MVC para satisfacer el spec sin añadir endpoint WebApi nuevo (superficie mínima). Se descarta (a) por no cumplir literalmente el spec; se descarta (b) por datos potencialmente desincronizados (proposal ya eligió opción b).

### D4 — Guardar con confirmación (reconciliación `ModelResponse<Paciente>`)

**Choice**: `guardarPacienteModal()`: `$('#pacienteForm').valid()` → `Swal.fire({title:'¿Confirmás guardar los cambios?', showCancelButton:true, …})` → si confirma, `POST guardarUrl` con `$('#pacienteForm').serialize()`. En éxito lee el Id de la entidad retornada: `var nuevoId = r.Response ? r.Response.Id : 0;` (la WebApi ya devuelve la entidad con `Id`, no un `int`). Luego `Swal` success → `$('#modalPaciente').modal('hide')` → `refrescarFila(nuevoId)`.

**Alternativas**: leer `r.Response` como `int` (rompería, es `Paciente`); usar `window.location`/`ajax.reload()` (prohibido por spec).

**Rationale**: el contrato real es `ModelResponse<Paciente>` (verificado en `DbWrapper.Paciente.cs:124` `ExecuteScalar` → `p.Id = Convert.ToInt32(id)`). La confirmación previa cumple el spec ("¿Confirmás guardar los cambios?").

### D5 — Refresco de SOLO la fila (DataTables 1.13.6)

**Choice**: al abrir el modal se guarda `var tr = $(btn).closest('tr')` en `currentTr` (null en alta). Tras guardar, `refrescarFila(id)` hace `$.get(consultarUrl,{id})` para obtener el DTO fresco y luego:
```js
if (currentTr) {
    var row = table.row(currentTr);
    if (row.data()) row.data(dto).invalidate().draw(false);
    else table.row.add(dto).draw(false);
} else {
    table.row.add(dto).draw(false); // alta (botón Nuevo Paciente, sin tr)
}
```
`NombreCompleto`/`Edad` vienen del re-fetch (D3). `draw(false)` conserva paginación/orden. **Prohibido** `ajax.reload()` y `window.location`.

**Alternativas**: `ajax.reload()` (recarga toda la tabla, viola spec); `table.cell(row, idx).data(...)` campo a campo (frágil).

**Rationale**: DataTables 1.13.6 con `processing` client-side (no server-side) soporta `row().data().invalidate().draw()` y `row.add().draw()` sin romper la fuente ajax. Capturar `tr` al abrir evita desincronización tras ordenar/filtrar.

### D6 — Eliminar (baja lógica) replicando el patrón de Cita

**Choice**: botón Eliminar **por fila** en la columna Acciones (no dentro del modal), con delegación de eventos DataTables. Flujo completo idéntico a `EliminarCita`:
- **SQL**: `sp_nh_DeletePaciente` en `sql/sp_nh_Paciente.sql` (CREATE + `IF OBJECT_ID('dbo.sp_nh_DeletePaciente','P') IS NULL` + `EXEC('…')`): `@Id INT` → `UPDATE dbo.Paciente SET Estatus = 0 WHERE Id = @Id;`.
- **WebApi**: `DbWrapper.Paciente.cs::EliminarPaciente(int id, string usuario)` → `ExecuteNonQuery("sp_nh_DeletePaciente", CommandType.StoredProcedure, new[]{ new SqlParameter("@Id", id) })`; `filas<=0` → `IsSuccess=false, "Paciente no encontrado."`. `Services/PacienteService.cs::EliminarPaciente(int id, string usuario)` con guard `id<=0 → IsSuccess=false`. `Controllers/PacienteController.cs`: `[HttpPost, Route("Eliminar/{id:int}")] ModelResponse<bool> EliminarPaciente(int id)`.
- **MVC**: `DAL/HttpClientConnection.Paciente.cs::EliminarPaciente(long id)` → `RequestAsync<bool>("api/Paciente/Eliminar/" + id, HttpMethod.Post, null, token.Token.access_token)`. `Services/PacienteService.cs::EliminarPaciente(long id)`. `Controllers/PacienteController.cs`: `[HttpPost] Task<string> EliminarPaciente(long id)` → `JsonConvert.SerializeObject(...)`.
- **Frontend**: `Swal.fire({title:'¿Estás seguro que quieres eliminar a este paciente?', icon:'warning', confirmButtonColor:'#DC3545', showCancelButton:true})` → `POST eliminarUrl {id}` → éxito → `table.row(tr).remove().draw(false)`.

**Alternativas**: botón dentro del modal (proposal lo descartó, y `draw(false)` necesita el `tr` de la fila); baja física (`DELETE`) — descartado, es baja lógica.

**Rationale**: reutiliza el contrato y los nombres exactos de `sp_nh_DeleteCita`/`EliminarCita` (consistencia de código); el guard `id<=0` y el chequeo `filas<=0` cubren el escenario "id inválido o inexistente".

### D7 — Registro en csproj + BOM UTF-8

**Choice**: añadir `<Content Include="Views\Paciente\_Formulario.cshtml" />` en `ConsultorioMedicoMVC.csproj` (junto a `Views\Paciente\Nuevo.cshtml`, línea 125). No hay `.cs` nuevos (solo ediciones de archivos existentes), salvo que el mapper de D3 se coloque en un archivo nuevo — recomendado mantenerlo dentro de `Services/PacienteService.cs` existente. Todos los `.cshtml`/`.cs` editados se guardan con **BOM UTF-8**.

**Alternativas**: crear `_Formulario.cshtml` sin registrarlo (no se desplegaría con WebDeploy); archivo `.cs` nuevo para el mapper (evitable).

**Rationale**: los `.cshtml` en MVC5 se despliegan por lista explícita de `<Content>`; omitirlo rompe el deploy. BOM UTF-8 evita acentos corruptos (gotcha de proyecto).

### D8 — Verificación

**Choice**: (1) MSBuild vía VsDevCmd + `aspnet_compiler` exit 0 (ver `config.yaml` `test_runner.command`); (2) ejecutar `sql/sp_nh_Paciente.sql` en BD productiva (CREATE idempotente) y validar `OBJECT_ID('dbo.sp_nh_DeletePaciente')` no nulo; (3) prueba manual: modal abre alta/edición, guarda con confirmación refrescando solo la fila, elimina con confirmación quitando solo la fila, rutas existentes (`Paciente/Nuevo/{id}`, `GetAllPacienteByEstatus`) intactas.

**Rationale**: no hay runner de tests (sin xUnit); la verificación es compilación + precompilado de vistas + prueba manual (PLAYBOOK §9). El SP MUST ejecutarse antes del despliegue (dependencia del proposal).

---

## Diagramas de secuencia

### 1) Abrir modal (edición)

```
Usuario ──clic "Editar"──▶ Index.cshtml (btn-editar-paciente[data-id])
   │  $(this).closest('tr') → tr ; currentTr = tr
   │  abrirModalPaciente(id, tr)  ──GET consultarUrl {id}──▶ PacienteController.ConsultarPacientePorId
   │        PacienteService.ConsultarPacientePorId ──▶ HttpClientConnection.ObtenerPacientePorId ──▶ api/Paciente/{id}
   │        (WebApi) PacienteService.ObtenerPacientePorId ──▶ DbWrapper (sp_nh_GetPacienteById) ──▶ BD
   │◀── ModelResponse<PacienteDTO> {NombreCompleto, Edad, …} ──poblarForm(dto)──
   │  $('#modalPaciente').modal('show')
```
En alta: `abrirModalPaciente(0, null)` → `resetForm()` + `Id=0` → `show` (sin GET).

### 2) Guardar (confirmar → POST → re-fetch → actualizar fila)

```
Usuario ──clic "Guardar"──▶ guardarPacienteModal()
   │  $('#pacienteForm').valid()  (jQuery Validate)
   │  Swal confirm "¿Confirmás guardar los cambios?" ──▶ (si cancela: fin)
   │  POST guardarUrl (serialize) ──▶ PacienteController.GuardarOActualizarPaciente
   │        → HttpClientConnection.GuardarOActualizarPaciente ──▶ api/Paciente/Guardar
   │        → DbWrapper.GuardarOActualizarPaciente (sp_nh_SavePaciente) ──▶ BD
   │◀── ModelResponse<Paciente> {Response.Id} ──
   │  Swal success → modal.hide() → refrescarFila(nuevoId)
   │        GET consultarUrl {nuevoId} ──▶ ModelResponse<PacienteDTO> (dto)
   │        row.data(dto).invalidate().draw(false)   (edición)
   │        o table.row.add(dto).draw(false)          (alta)
```

### 3) Eliminar

```
Usuario ──clic "Eliminar"──▶ (btn-eliminar-paciente) eliminarPaciente(id, tr)
   │  Swal confirm "¿Estás seguro que quieres eliminar a este paciente?" ──▶ (si cancela: fin)
   │  POST eliminarUrl {id} ──▶ PacienteController.EliminarPaciente
   │        → HttpClientConnection.EliminarPaciente ──▶ api/Paciente/Eliminar/{id}
   │        → DbWrapper.EliminarPaciente (sp_nh_DeletePaciente: UPDATE Estatus=0) ──▶ BD
   │◀── ModelResponse<bool> ──
   │  Swal success → table.row(tr).remove().draw(false)
```

---

## Detalle por capa

### Vista (`ConsultorioMedicoMVC/Views/Paciente/`)

- **`_Formulario.cshtml`** (NUEVO): `@model Paciente`; `<form id="pacienteForm" autocomplete="off" novalidate>` con hidden `Id, Estatus, FechaAlta, CreatedBY, CreatedDT, UpdatedBY, UpdatedDT, Edad` + visibles `Nombre, ApellidoPaterno, ApellidoMaterno, Sexo, FechaNacimiento(date), TelefonoCel, Email, Comentarios`. Sin botones ni `<script>`.
- **`Nuevo.cshtml`** (MOD): wrapper `@Html.Partial("_Formulario", Model)` + botones (Cancelar→Index, Guardar→`guardarPaciente()` full-page que conserva `window.location`) + `@section scripts` con `$.validator.addMethod("telefono10",…)` + `.validate({rules/messages})` + `guardarPaciente()` (sin cambios de comportamiento).
- **`Index.cshtml`** (MOD): botón "Nuevo Paciente" → `<button onclick="abrirModalPaciente(0,null)">`; modal `#modalPaciente` (D2); columna Acciones renderiza `<button class="btn btn-warning btn-editar-paciente" data-id>` y `<button class="btn btn-danger btn-eliminar-paciente" data-id>` (además del enlace Expediente). `@section scripts`: `var table = $('#pacientesTable').DataTable({…})`, URLs, validación, `abrirModalPaciente`, `poblarForm`, `resetForm`, `guardarPacienteModal`, `refrescarFila`, `eliminarPaciente`, delegación `$('#pacientesTable').on('click', '.btn-editar-paciente' | '.btn-eliminar-paciente', …)`.

### MVC (`ConsultorioMedicoMVC/`)

- `Controllers/PacienteController.cs`: `ConsultarPacientePorId(long id)` (JSON), `[HttpPost] EliminarPaciente(long id)` (JSON).
- `Services/PacienteService.cs`: `ConsultarPacientePorId(long id)` (mapea `Paciente`→`PacienteDTO` + `NombreCompleto`), `EliminarPaciente(long id)`.
- `DAL/HttpClientConnection.Paciente.cs`: `EliminarPaciente(long id)` → `api/Paciente/Eliminar/{id}`.
- `ConsultorioMedicoMVC.csproj`: `<Content Include="Views\Paciente\_Formulario.cshtml" />`.
- `Content/site.css`: regla `.modal-xl .modal-body { max-height: 70vh; overflow-y: auto; }`.

### WebApi (`ConsultorioMedicoWebApi/`)

- `Controllers/PacienteController.cs`: `[HttpPost, Route("Eliminar/{id:int}")] ModelResponse<bool> EliminarPaciente(int id)`.
- `Services/PacienteService.cs`: `EliminarPaciente(int id, string usuario)` (guard `id<=0`).
- `DAL/DbWrapper.Paciente.cs`: `EliminarPaciente(int id, string usuario)` → `ExecuteNonQuery("sp_nh_DeletePaciente", …, new[]{ @Id })`.

### SQL (`sql/sp_nh_Paciente.sql`)

- Añadir bloque `IF OBJECT_ID('dbo.sp_nh_DeletePaciente','P') IS NULL … EXEC('CREATE PROCEDURE dbo.sp_nh_DeletePaciente (@Id INT) AS BEGIN SET NOCOUNT ON; UPDATE dbo.Paciente SET Estatus = 0 WHERE Id = @Id; END') … GO`.

---

## Consideraciones de verificación

| Capa | Qué validar | Cómo |
|------|-------------|------|
| Compilación | Solución + vistas | MSBuild (VsDevCmd) + `aspnet_compiler` exit 0 |
| BD | SP existe y es idempotente | Ejecutar `sp_nh_Paciente.sql`; `SELECT OBJECT_ID('dbo.sp_nh_DeletePaciente')` |
| UI alta | Modal abre vacío, valida, confirma, inserta fila | Manual: "Nuevo Paciente" → Guardar |
| UI edición | Modal puebla datos frescos, guarda, refresca solo esa fila | Manual: Editar → Guardar |
| UI baja | Confirmación, quita solo la fila, Estatus=0 en BD | Manual: Eliminar |
| No regresión | `Paciente/Nuevo/{id}`, listado, validación | Manual + compilación |

**Gotchas**: BOM UTF-8; Razor C# 5 (sin `?.`/`$""` en `.cshtml`); MVC usa form-data (`serialize()`); JSON MVC es PascalCase (`IsSuccess/Response/NombreCompleto`), WebApi camelCase (transparente al cliente MVC vía `RequestAsync`); el partial no puede tener `<script>` (jQuery carga después de `@RenderBody()`).

## Preguntas abiertas

- Ninguna que bloquee. (Único punto sensible: ejecutar el SP en BD productiva antes del despliegue, ya contemplado en D8.)
