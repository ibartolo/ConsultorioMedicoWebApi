# Tasks: Paciente — CRUD en modales + baja lógica

## Fase 1: SQL

- [x] 1.1 `sql/sp_nh_Paciente.sql`: anexar bloque `sp_nh_DeletePaciente` al final — `IF OBJECT_ID('dbo.sp_nh_DeletePaciente','P') IS NULL` + `EXEC('CREATE PROCEDURE ...')`; `@Id INT` → `SET NOCOUNT ON; UPDATE dbo.Paciente SET Estatus=0 WHERE Id=@Id;`. Verificar: CREATE idempotente (sin DROP/ALTER).
- [x] 1.2 `sql/sp_nh_Paciente.sql`: listar `sp_nh_DeletePaciente` en el comentario de cabecera.

## Fase 2: WebApi

- [x] 2.1 `ConsultorioMedicoWebApi/DAL/DbWrapper.Paciente.cs`: `public ModelResponse<bool> EliminarPaciente(int id, string usuario)` → `ExecuteNonQuery("sp_nh_DeletePaciente", CommandType.StoredProcedure, new[]{ new SqlParameter("@Id", id) })`; `filas<=0` → `IsSuccess=false` + "Paciente no encontrado."; else `Response=true`.
- [x] 2.2 `ConsultorioMedicoWebApi/Services/PacienteService.cs`: `EliminarPaciente(int id, string usuario)` con guard `id<=0` → `IsSuccess=false`; try/catch → `ModelResponse<bool>` en español.
- [x] 2.3 `ConsultorioMedicoWebApi/Controllers/PacienteController.cs`: `[HttpPost, Route("Eliminar/{id:int}")] ModelResponse<bool> EliminarPaciente(int id)` → `_pacienteService.EliminarPaciente(id, User.Identity.Name)`.

## Fase 3: MVC

- [x] 3.1 `ConsultorioMedicoMVC/DAL/HttpClientConnection.Paciente.cs`: `EliminarPaciente(long id)` → `RequestAsync<bool>("api/Paciente/Eliminar/" + id, HttpMethod.Post, null, token.Token.access_token)`.
- [x] 3.2 `ConsultorioMedicoMVC/Services/PacienteService.cs`: `EliminarPaciente(long id)` → `_httpClient.EliminarPaciente(id)`.
- [x] 3.3 `ConsultorioMedicoMVC/Services/PacienteService.cs`: `ConsultarPacientePorId(long id)` — `ObtenerPacientePorId` → mapea `Paciente`→`PacienteDTO`, `NombreCompleto = ($"{p.Nombre} {p.ApellidoPaterno} {p.ApellidoMaterno}").Trim()`; `!IsSuccess` → "Paciente no encontrado.".
- [x] 3.4 `ConsultorioMedicoMVC/Controllers/PacienteController.cs` (Data Access): `[HttpPost] Task<string> EliminarPaciente(long id)` → `JsonConvert.SerializeObject(...)`.
- [x] 3.5 `ConsultorioMedicoMVC/Controllers/PacienteController.cs` (Data Access): `Task<string> ConsultarPacientePorId(long id)` → `JsonConvert.SerializeObject(...)`.

## Fase 4: Partial view + csproj

- [x] 4.1 Crear `ConsultorioMedicoMVC/Views/Paciente/_Formulario.cshtml` (`@model Paciente`): `<form id="pacienteForm" autocomplete="off" novalidate>` con 8 hidden (Id, Estatus, FechaAlta, CreatedBY, CreatedDT, UpdatedBY, UpdatedDT, Edad) + visibles (Nombre, ApellidoPaterno, ApellidoMaterno, Sexo, FechaNacimiento `type="date"`, TelefonoCel, Email, Comentarios). Sin botones ni `<script>`. BOM UTF-8.
- [x] 4.2 `ConsultorioMedicoMVC/Views/Paciente/Nuevo.cshtml`: reemplazar form por `@Html.Partial("_Formulario", Model)` + conservar botones y `@section scripts`.
- [x] 4.3 `ConsultorioMedicoMVC/ConsultorioMedicoMVC.csproj`: añadir `<Content Include="Views\Paciente\_Formulario.cshtml" />` tras línea 125.
- [x] 4.4 `ConsultorioMedicoMVC/Content/site.css`: añadir `.modal-xl .modal-body { max-height: 70vh; overflow-y: auto; }`.

## Fase 5: Index / Modal / JS

- [x] 5.1 `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml`: añadir `#modalPaciente` (`modal-xl modal-dialog-scrollable`) renderizando `@Html.Partial("_Formulario", new Paciente())`; footer Cancelar + Guardar (`onclick="guardarPacienteModal()"`).
- [x] 5.2 `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml`: botón "Nuevo Paciente" → `onclick="abrirModalPaciente(0,null)"`; Acciones: Editar `<button class="btn-warning btn-editar-paciente" data-id>` + Eliminar `<button class="btn-danger btn-eliminar-paciente" data-id>` (fa-trash).
- [x] 5.3 `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml` JS: URLs (`consultarUrl`, `guardarUrl`, `eliminarUrl`), `table` DataTable, validación (telefono10 + rules/messages), delegación `.btn-editar-paciente` / `.btn-eliminar-paciente`.
- [x] 5.4 `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml` JS: `abrirModalPaciente(id, tr)` — `id>0` → `$.get(consultarUrl,{id})` → `poblarForm` (fallback `table.row(tr).data()`); `id=0` → `resetForm()` + `Id=0`; guarda `currentTr`; abre modal.
- [x] 5.5 `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml` JS: `poblarForm`/`resetForm` con fechas `yyyy-MM-dd` (FechaNacimiento) y `yyyy-MM-dd HH:mm:ss` (hidden).
- [x] 5.6 `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml` JS: `guardarPacienteModal()` — `.valid()` → Swal confirm → POST `guardarUrl` (serialize) → `r.Response.Id` → `refrescarFila` → cerrar. Sin `window.location`/`ajax.reload()`.
- [x] 5.7 `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml` JS: `refrescarFila(id)` — `$.get(consultarUrl,{id})` → `currentTr ? row.data(dto).invalidate().draw(false) : table.row.add(dto).draw(false)`.
- [x] 5.8 `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml` JS: `eliminarPaciente(id, tr)` — Swal confirm (`#DC3545`) → POST `eliminarUrl {id}` → `table.row(tr).remove().draw(false)`.

## Fase 6: Verificación

- [x] 6.1 MSBuild (VsDevCmd) + `aspnet_compiler` → exit 0.
- [x] 6.2 Ejecutar `sql/sp_nh_Paciente.sql` en BD Smarterasp → `SELECT OBJECT_ID('dbo.sp_nh_DeletePaciente')` no nulo.
- [ ] 6.3 Prueba manual: alta/edición/eliminación desde modal refrescan solo la fila; rutas `Paciente/Nuevo/{id}` y `GetAllPacienteByEstatus` intactas.
