# Tasks: Calendario de Citas — Estados, Colores y Eliminación

## Phase 1: Fix Consultas

- [x] 1.1 `ConsultorioMedicoMVC/Views/Consulta/Index.cshtml` (línea 119): `initialView: 'timeGridWeek` → `initialView: 'timeGridWeek',` (agregar coma final). Verificar: `<script>` sin error de sintaxis JS.

## Phase 2: SQL

- [x] 2.1 `sql/sp_nh_Cita.sql`: anexar 4º bloque `sp_nh_DeleteCita` al final — `IF OBJECT_ID('dbo.sp_nh_DeleteCita','P') IS NULL` + `EXEC('CREATE PROCEDURE ...')`; `@Id INT` → `SET NOCOUNT ON; UPDATE dbo.Cita SET Estatus=0 WHERE Id=@Id;`. Verificar: CREATE idempotente, sin DROP/ALTER.
- [x] 2.2 `sql/sp_nh_Cita.sql`: actualizar comentario de cabecera para listar `sp_nh_DeleteCita`.

## Phase 3: WebApi

- [x] 3.1 `ConsultorioMedicoWebApi/DAL/DbWrapper.Cita.cs`: método `public ModelResponse<bool> EliminarCita(int id, string usuario)` → `ExecuteNonQuery("sp_nh_DeleteCita", CommandType.StoredProcedure, new[]{ new SqlParameter("@Id", id) })`; `filas<=0`→`IsSuccess=false` + `"Cita no encontrada."`; else `IsSuccess=true` + `Response=true`.
- [x] 3.2 `ConsultorioMedicoWebApi/Services/CitaService.cs`: método `public ModelResponse<bool> EliminarCita(int id, string usuario)`; valida `id<=0`→error; try/catch→`ModelResponse<bool>` con mensajes en español.
- [x] 3.3 `ConsultorioMedicoWebApi/Controllers/CitaController.cs`: `[HttpPost, Route("Eliminar/{id:int}")] public ModelResponse<bool> EliminarCita(int id) => _citaService.EliminarCita(id, User.Identity.Name);`

## Phase 4: MVC

- [x] 4.1 `ConsultorioMedicoMVC/DAL/HttpClientConnection.Cita.cs`: método `public async Task<ModelResponse<bool>> EliminarCita(long id) => await RequestAsync<bool>("api/Cita/Eliminar/" + id, HttpMethod.Post, null, token.Token.access_token);`
- [x] 4.2 `ConsultorioMedicoMVC/Services/CitaService.cs`: método `public async Task<ModelResponse<bool>> EliminarCita(long id) => await _httpClient.EliminarCita(id);`
- [x] 4.3 `ConsultorioMedicoMVC/Controllers/CitaController.cs` (región Data Access): `[HttpPost] public async Task<string> EliminarCita(long id) => JsonConvert.SerializeObject(await _citaService.EliminarCita(id));`

## Phase 5: Vista Cita

- [x] 5.1 `ConsultorioMedicoMVC/Views/Cita/Index.cshtml`: función `colorEstado(e)` → `{ backgroundColor, borderColor, textColor }` (1=#FD7E14, 2=#198754, 3=#DC3545, null/otro=#6C757D; texto #FFFFFF).
- [x] 5.2 `ConsultorioMedicoMVC/Views/Cita/Index.cshtml`: función `nombreEstado(e)` → 1=Pendiente, 2=Confirmada, 3=Cancelada, default='Sin estado'.
- [x] 5.3 `ConsultorioMedicoMVC/Views/Cita/Index.cshtml`: en `events.push({...})` aplicar `backgroundColor/borderColor/textColor: colorEstado(c.Estado)`; agregar `eventDidMount` que setea `info.el.title = 'Estado: ' + nombreEstado(p.Estado)` (+ `p.PacienteNombre`/`p.TratamientoNombre` si existen).
- [x] 5.4 `ConsultorioMedicoMVC/Views/Cita/Index.cshtml`: botón `#btnEliminarCita` (btn-danger) en el footer del modal `#modalCita`; `abrirModalCita(c)` toggle visible solo si `c && c.Id>0`; oculto/reset para cita nueva.
- [x] 5.5 `ConsultorioMedicoMVC/Views/Cita/Index.cshtml`: `var eliminarUrl = '@Url.Action("EliminarCita","Cita")'`; `eliminarCita()` → `Swal.fire` confirmación (`confirmButtonColor:'#DC3545'`) → `$.ajax POST eliminarUrl` con `data:{ id: $('#Id').val() }` → éxito: Swal + `$('#modalCita').modal('hide')` + `calendar.refetchEvents()`.

## Phase 6: Verificación / Operación BD

- [x] 6.1 Ejecutar `sql/sp_nh_Cita.sql` contra BD productiva (connection string `cCon` en Web.config WebApi) → crea `sp_nh_DeleteCita`. Verificar: `SELECT name FROM sys.procedures WHERE name='sp_nh_DeleteCita'` retorna fila.
- [x] 6.2 Ejecutar `sql/sp_nh_Historial.sql` contra BD productiva → crea `sp_nh_GetTratamientosPorPaciente`. Verificar: `SELECT name FROM sys.procedures WHERE name='sp_nh_GetTratamientosPorPaciente'` retorna fila.
- [x] 6.3 Build: MSBuild `DESI.ConsultorioMedico.sln` (Debug) exit 0 + `aspnet_compiler` (PLAYBOOK §9) exit 0.
- [ ] 6.4 E2E: calendario Consultas renderiza; calendario Citas pinta por estado + tooltip; "Eliminar" visible solo al editar; confirmar → cita desaparece (`refetchEvents`); grid "Tratamientos Históricos" muestra datos.
