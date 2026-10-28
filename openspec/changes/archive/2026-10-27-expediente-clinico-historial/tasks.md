# Tasks: Expediente / Historial Clínico del Paciente

## Phase 1: SQL

- [x] 1.1 Crear `sql/sp_nh_Historial.sql` con `sp_nh_GetTratamientosPorPaciente` (`@IdPaciente INT` → `Id, Fecha, TratamientoNombre, Origen, Comentarios`): `UNION` de Cita + Consulta con `LEFT JOIN Tratamiento`, `WHERE Estatus = 1`, `ORDER BY Fecha DESC`. Solo `CREATE` con `IF OBJECT_ID(...) IS NULL` (estilo `sp_nh_Cita.sql`). Verificar: patrón `IF OBJECT_ID` + `PRINT`, sin DROP/ALTER.

## Phase 2: Entities

- [x] 2.1 Crear `ConsultorioMedicoEntities/Catalogos/TratamientoHistoricoDTO.cs` — POCO standalone (estilo `RecetaDTO`) con `int Id`, `DateTime? Fecha`, `string TratamientoNombre`, `string Origen`, `string Comentarios`.
- [x] 2.2 Registrar `<Compile Include="Catalogos\TratamientoHistoricoDTO.cs" />` en `ConsultorioMedicoEntities.csproj`. Verificar: compila Entities exit 0.

## Phase 3: WebApi

- [x] 3.1 Crear `ConsultorioMedicoWebApi/DAL/DbWrapper.Historial.cs` (partial `DbWrapper`): `ModelResponse<List<TratamientoHistoricoDTO>> ObtenerTratamientosPorPaciente(long pacienteId, string usuario)` → `GetObjects("sp_nh_GetTratamientosPorPaciente", mapper, CommandType.StoredProcedure, new[] { new SqlParameter("@IdPaciente", pacienteId) })`; mapper con `Convert.IsDBNull` para `Fecha` (estilo `DbWrapper.Cita.cs`).
- [x] 3.2 Crear `ConsultorioMedicoWebApi/Services/HistorialService.cs`: `new DbWrapper()`; try/catch + Serilog; guard `if (pacienteId <= 0) return IsSuccess=false` + mensaje español (D7).
- [x] 3.3 Crear `ConsultorioMedicoWebApi/Controllers/HistorialController.cs`: `[Authorize] [RoutePrefix("api/Historial")]`, hereda `BaseController`; endpoint `[HttpGet, Route("TratamientosPorPaciente/{pacienteId:long}")]` → `ModelResponse<List<TratamientoHistoricoDTO>>`.
- [x] 3.4 Registrar 3 `<Compile>` (controller, service, `DbWrapper.Historial.cs`) en `ConsultorioMedicoWebApi.csproj`.

## Phase 4: MVC

- [x] 4.1 Crear `ConsultorioMedicoMVC/DAL/HttpClientConnection.Historial.cs` (partial): `ObtenerTratamientosPorPaciente(long pacienteId)` → `RequestAsync<List<TratamientoHistoricoDTO>>("api/Historial/TratamientosPorPaciente/" + pacienteId, HttpMethod.Get, null, token.Token.access_token)`.
- [x] 4.2 Crear `ConsultorioMedicoMVC/Services/HistorialService.cs`: ctor `(HttpClientConnection httpClient)`; `ConsultarTratamientosPorPaciente(long pacienteId)`.
- [x] 4.3 Modificar `ConsultorioMedicoMVC/Controllers/HistorialController.cs`: añadir `_historialService` y reorganizar en `#region Views`/`#region Data Access` (estilo `PacienteController`). `Views`: `Expediente(long id)` (carga `_pacienteService.ObtenerPacientePorId(id)`, `RedirectToAction("Index","Paciente")` si null, `return View(p)`). `Data Access`: `ConsultarTratamientosPorPaciente(long pacienteId)` → `JsonConvert.SerializeObject(...)`.
- [x] 4.4 Registrar 2 `<Compile>` (`HttpClientConnection.Historial.cs`, `Services\HistorialService.cs`) en `ConsultorioMedicoMVC.csproj`.

## Phase 5: Vistas

- [x] 5.1 Crear `ConsultorioMedicoMVC/Views/Historial/Expediente.cshtml` (`@model ConsultorioMedicoEntities.Catalogos.Paciente`, BOM UTF-8, Razor C# 5 sin `?.` ni `$""`): 4 pestañas `nav-tabs`. Tab1 panel info (nombre completo compuesto `@Model.Nombre @Model.ApellidoPaterno @Model.ApellidoMaterno` + Email/TelefonoCel/Sexo/Edad/FechaNacimiento) + DataTable tratamientos (AJAX `ConsultarTratamientosPorPaciente`, cols Fecha/TratamientoNombre/Origen/Comentarios). Tab2 DataTable consultas (AJAX `ConsultarConsultasPorPaciente`; `dblclick` fila → `window.open('Consulta/Receta/' + id, '_blank')`). Tab3 Archivos y Tab4 Paquetes con `d-none`. Inyectar `@Model.Id` como `pacienteId` en `data` de los AJAX.
- [x] 5.2 Registrar `<Content Include="Views\Historial\Expediente.cshtml" />` en `ConsultorioMedicoMVC.csproj`.

## Phase 6: Grid Pacientes

- [x] 6.1 Modificar `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml`: columna Acciones con 2 botones (editar existente + `<a href="@Url.Action("Expediente","Historial")/{id}" ...>Expediente Clínico</a>`).

## Phase 7: Verificación

- [x] 7.1 MSBuild solución Debug (`DESI.ConsultorioMedico.sln`) → exit 0.
- [x] 7.2 `aspnet_compiler` precompilando vistas → exit 0 (valida `Expediente.cshtml` Razor C# 5 y `Paciente/Index.cshtml`).
