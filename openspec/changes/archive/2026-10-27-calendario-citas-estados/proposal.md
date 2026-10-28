# Proposal: Calendario de Citas — Estados, Colores y Eliminación

## Intent

(1) Corregir el bug de sintaxis JS que rompe el calendario de Consultas; (2) pintar los eventos del calendario de Citas según `Estado` (1 Pendiente, 2 Confirmada, 3 Cancelada) con tooltip; (3) habilitar la baja lógica de una Cita (`Estatus=0`) con botón "Eliminar" — patrón nuevo de borrado, hoy inexistente en ningún controller; (4) ejecutar el SP `sp_nh_GetTratamientosPorPaciente` (faltante en BD productiva) para que el grid "Tratamientos Históricos" deje de estar vacío.

## Scope

### In Scope
- Fix `ConsultorioMedicoMVC/Views/Consulta/Index.cshtml` (línea 119): `initialView: 'timeGridWeek` → `initialView: 'timeGridWeek',`.
- `ConsultorioMedicoMVC/Views/Cita/Index.cshtml`: mapear `Estado` → color/tooltip (eventos ya traen `extendedProps.Estado`).
- Baja lógica de Cita: SP `sp_nh_DeleteCita` + endpoint WebApi `POST api/Cita/Eliminar` + acción MVC `EliminarCita` + botón/confirmación/`refetchEvents`.
- Ejecutar `sql/sp_nh_Historial.sql` (CREATE idempotente) en BD productiva.

### Out of Scope
- Expediente (modales ya funcionan), `Historial/Index`, tabs ocultas.
- Eliminar de Consulta (no pedido).
- Validación de choque de horarios.
- DROP/ALTER en BD (solo CREATE con `IF OBJECT_ID`).

## Capabilities

### New Capabilities
- `cita`: comportamiento del calendario de Citas — colores + tooltip según `Estado` y baja lógica (`sp_nh_DeleteCita` + endpoint `Eliminar`).

### Modified Capabilities
- None. (El fix de Consulta y el SP de historial son bugfix/ejecución, no cambios de spec.)

## Approach

**SQL**: anexar `sp_nh_DeleteCita` a `sql/sp_nh_Cita.sql` (`@Id INT` → `UPDATE dbo.Cita SET Estatus=0 WHERE Id=@Id`; `CREATE` + `IF OBJECT_ID`). Debe ejecutarse contra BD productiva (idempotente).

**Entities**: sin cambios (`Cita`/`CitaDTO` ya exponen `Estado int?`).

**WebApi**:
- `DAL/DbWrapper.Cita.cs` → `EliminarCita(long id, string usuario)` (ExecuteNonQuery) → `ModelResponse<int>`.
- `Services/CitaService.cs` → `EliminarCita(long id, string usuario)`.
- `Controllers/CitaController.cs` → `[HttpPost, Route("Eliminar")] EliminarCita(long id)`.

**MVC**:
- `DAL/HttpClientConnection.Cita.cs` → `EliminarCita(long id)` → `RequestAsync<int>("api/Cita/Eliminar", HttpMethod.Post, new { id }, token)`.
- `Services/CitaService.cs` → `EliminarCita(long id)`.
- `Controllers/CitaController.cs` (región Data Access) → `[HttpPost] EliminarCita(long id)` → `JsonConvert`.
- `Views/Cita/Index.cshtml`: helper `colorEstado(estado)` → `{backgroundColor, borderColor, textColor}` (1=#FD7E14, 2=#198754, 3=#DC3545, null/otro=#6C757D); tooltip vía `eventDidMount` (`info.el.title = nombreEstado`). Botón "Eliminar" en footer del modal (visible solo si `Id>0`) + `eliminarCita()` con SweetAlert confirmación → POST → `calendar.refetchEvents()`.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `ConsultorioMedicoMVC/Views/Consulta/Index.cshtml` | Modified | Fix `initialView: 'timeGridWeek',` |
| `ConsultorioMedicoMVC/Views/Cita/Index.cshtml` | Modified | Colores + tooltip + botón Eliminar + JS |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.Cita.cs` | Modified | `EliminarCita` (ExecuteNonQuery) |
| `ConsultorioMedicoWebApi/Services/CitaService.cs` | Modified | `EliminarCita` |
| `ConsultorioMedicoWebApi/Controllers/CitaController.cs` | Modified | `[HttpPost, Route("Eliminar")]` |
| `ConsultorioMedicoMVC/DAL/HttpClientConnection.Cita.cs` | Modified | `EliminarCita` |
| `ConsultorioMedicoMVC/Services/CitaService.cs` | Modified | `EliminarCita` |
| `ConsultorioMedicoMVC/Controllers/CitaController.cs` | Modified | Acción `EliminarCita` |
| `sql/sp_nh_Cita.sql` | Modified | SP `sp_nh_DeleteCita` |
| `sql/sp_nh_Historial.sql` | Ejecutar | `sp_nh_GetTratamientosPorPaciente` en BD |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| SPs nuevos requieren ejecución en BD productiva (Smarterasp) | High | Documentar en tasks/verify; CREATE idempotente no daña nada |
| Binding de `long id` en POST `Eliminar` (JSON vs query) | Low | Definir en design; probar con Postman/MVC |
| Tooltip/colores no afectan datos (solo UI) | Low | Sin impacto en BD |
| `Estado` null (citas sin estado) | Low | Color default gris + tooltip "Sin estado" |

## Rollback Plan

- UI (colores/tooltip/botón): `git checkout` de las dos vistas; sin efecto en datos.
- Backend delete: `git checkout` de DbWrapper/Service/Controller/DAL MVC; el SP `sp_nh_DeleteCita` es idempotente y solo se usa si se invoca (no afecta listados que filtran `Estatus=1`).
- SP historial: ejecutar el CREATE no altera objetos existentes.

## Dependencies

- SPs existentes `sp_nh_SaveCita`, `sp_nh_GetAllCitas` (listados filtran `Estatus=1`).
- BD `DB_9C7990_Consultorio` accesible para ejecutar `sp_nh_Historial.sql` y `sp_nh_DeleteCita`.

## Success Criteria

- [ ] MSBuild Debug + `aspnet_compiler` exit 0.
- [ ] Calendario de Consultas renderiza (bug sintaxis corregido).
- [ ] Calendario de Citas pinta por estado + tooltip correcto.
- [ ] "Eliminar" visible solo al editar (`Id>0`); SweetAlert confirma y la cita desaparece (`refetchEvents`).
- [ ] Grid "Tratamientos Históricos" muestra datos tras ejecutar `sp_nh_GetTratamientosPorPaciente`.

## Notas

- Patrón de borrado nuevo: `ModelResponse<int>` (HTTP 200 siempre), baja lógica `Estatus=0`.
- FullCalendar 6.1.15: usar `eventDidMount` para tooltip (no `title` nativo, para HTML nativo del navegador).
- `CitaDTO : Cita` hereda `Estado int?` → se serializa como número en `extendedProps`.
