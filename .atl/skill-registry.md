# Skill Registry — ConsultorioMedico (MVC + WebApi)

> Refrescado el 2026-09-04: el repo migró de la fase greenfield (.NET 10 DESI) a la
> migración MVC 5 + WebApi sobre .NET Framework 4.8 (rama develop).

## User Skills (file-based)

Relevante para este proyecto (de `~/.config/opencode/skills/`):
- `netframework-mvc-webapi` — arquitectura de referencia ServiceDeskDESI: 3 proyectos
  (Entities / MVC front / WebApi back) sobre .NET Framework 4.8, login OAuth2 bearer en
  cookie FormsAuthentication, MVC service+DAL por HttpClient, WebApi con ADO.NET + SPs,
  envoltura `ModelResponse`. USAR para cualquier módulo/entidad/catálogo nuevo.

SDD framework skills (proporcionadas por el workflow, no son de proyecto):
`sdd-init`, `sdd-explore`, `sdd-propose`, `sdd-spec`, `sdd-design`, `sdd-tasks`,
`sdd-apply`, `sdd-verify`, `sdd-archive`, `sdd-onboard`.

## Project-Level Skills / Convenciones

- No hay skills de proyecto en `.agent/skills/`, `.claude/skills/`, etc.
- **`PLAYBOOK.md` (raíz del repo) es la convención de proyecto obligatoria**: arquitectura
  de capas, reglas de acceso a datos (NHibernate muere → SPs `sp_nh_*`), checklist de
  módulos, patrón de formularios (jQuery Validate + hidden inputs), validación en capas,
  gotchas (BOM UTF-8, Razor C#5 sin `?.`, form-data no JSON, binding redirects), comandos
  de verificación (MSBuild + aspnet_compiler).

## Compact Rules (auto-resolver — texto para inyectar a sub-agentes)

- **Stack**: .NET Framework 4.8; solución `DESI.ConsultorioMedico.sln` en la raíz con 3
  proyectos: `ConsultorioMedicoEntities` (entidades/DTOs/validadores), `ConsultorioMedicoWebApi`
  (WebApi OWIN+OAuth, ADO.NET), `ConsultorioMedicoMVC` (MVC 5, Bootstrap 5 + jQuery).
- **Flujo**: Vista (jQuery Validate + serialize) → Controller MVC (solo delega) → Servicio MVC
  (valida + "si es nuevo") → HttpClientConnection → Controller WebApi → Servicio WebApi (valida)
  → DbWrapper (SPs) → BD.
- **Data**: NHibernate muere → TODO por SPs. SP existente en WinForms → reutilizar. Si la
  WinForms usaba NHibernate/SQL inline → SP nuevo `sp_nh_` + nombre del método. SPs en
  `sql/*.sql` solo `CREATE` con `IF OBJECT_ID` (nunca DROP/ALTER). Upsert no escribe auditoría.
- **Entities**: `Catalogos/<Entidad>.cs` (BaseObject si auditoría; si no, POCO), `<Entidad>DTO.cs`
  (hereda entidad + campos display), `<Entidad>Validator.cs` estático compartido. Registrar en csproj.
- **WebApi**: `DAL/DbWrapper.<Entidad>.cs` partial (ObtenerX/ObtenerXPorId/GuardarOActualizarX,
  upsert con ExecuteScalar devuelve id) + `Services/<Entidad>Service.cs` (new DbWrapper; valida +
  "si es nuevo") + `Controllers/<Entidad>Controller.cs` (`[Authorize] [RoutePrefix("api/<Entidad>")]`
  List / {id:long} / Guardar). Registrar en csproj.
- **MVC**: `DAL/HttpClientConnection.<Entidad>.cs` partial (async → RequestAsync con
  token.Token.access_token) + `Services/<Entidad>Service.cs` (ctor HttpClientConnection; valida +
  "si es nuevo") + `Controllers/<Entidad>Controller.cs` (región Views `X(long id=0)` + región
  Data Access `Task<string>` → JsonConvert.SerializeObject). Registrar en csproj.
- **Vistas**: `Views/<Entidad>/Index.cshtml` (DataTable AJAX ConsultarTodosX) + `Nuevo.cshtml`
  (formulario jQuery Validate). Registrar cshtml como `<Content>` en csproj.
- **Formularios**: Bootstrap 5 + Font Awesome + SweetAlert2 + DataTables + jquery.validate 1.19.5
  en _Layout. Campos con `name`; datos no visibles en hidden inputs; fechas hidden con formato
  `yyyy-MM-dd HH:mm:ss`. Botón `type="button"` onclick guardarX(). AJAX con `serialize()`
  (form-data), NUNCA JSON.stringify. Enlace edición `/Controller/Accion/{id}`.
- **Validación en capas**: mismo validador estático Entities usado por MVC y WebApi. Lógica
  "si es nuevo" (`if (e.Id == 0) { e.Estatus = true; ... }`) en servicios, no en controller.
- **Respuesta**: `ModelResponse<T>` (IsSuccess / Message / Response), HTTP 200 siempre, errores
  con IsSuccess=false + Message en español. WebApi serializa camelCase; MVC PascalCase.
- **Id** = `int` (BD int IDENTITY, no long).
- **Gotchas**: BOM UTF-8 obligatorio en archivos con acentos (write tool NO agrega BOM →
  re-guardar con UTF8Encoding($true)). Razor = C# 5 → en .cshtml NO usar `?.` ni `$""`.
  MVC model binding = form-data. OAuth password grant con username/password minúscula.
  Binding redirects en Web.config (Owin 4.2.3.0, OAuth 3.0.1.0, Razor 3.0.0.0,
  Newtonsoft 13.0.0.0). `<globalization utf-8 es-MX>` y machineKey fija compartida.
- **Verificación**: MSBuild solución Debug + aspnet_compiler precompilando vistas, ambos exit 0.
- **Simulación**: ELIMINADA (2026-09-02); la app va directo a la BD real. No reintroducir.
- **BD**: productiva `DB_9C7990_Consultorio` (Smarterasp). NO eliminar nada. NO ejecutar SPs
  DROP/ALTER.

## Stack-Specific Notes (resumen para `apply`)

- C#: LangVersion 7.3 en .cs; Razor C# 5 en .cshtml.
- Paquetes clave WebApi: Microsoft.AspNet.WebApi 5.3.0, Owin 4.2.3, OAuth 3.0.1, Newtonsoft 13.0.3,
  Serilog 2.12.0. MVC: Microsoft.AspNet.Mvc 5.3.0.
- Sin proyectos de test (no xUnit). Verificación = compilar + precompilar vistas.
