# Proposal: Expediente / Historial Clínico del Paciente

## Intent

Dar al médico una vista consolidada de **solo lectura** del expediente de un paciente desde el grid de Pacientes: sus datos, sus tratamientos históricos (derivados de Cita+Consulta) y sus consultas con drill-down a la receta. Hoy `Historial/Index` es un selector con 3 DataTables sueltas, sin pestañas, sin panel de datos del paciente y sin historial de tratamientos.

## Scope

### In Scope
- Botón "Expediente Clínico" por fila en `Views/Paciente/Index.cshtml` → `/Historial/Expediente/{id}`.
- Nueva vista `Views/Historial/Expediente.cshtml` con 4 tabs Bootstrap 5 (`nav-tabs`):
  - Tab 1 "Información del paciente": panel (datos de `api/Paciente/{id}`) + grid de tratamientos derivados de Cita+Consulta.
  - Tab 2 "Historial de consultas": grid (`api/Consulta/PorPaciente/{id}`); doble clic → `Consulta/Receta/{id}` en nueva pestaña.
  - Tab 3 "Archivos": OCULTA. Tab 4 "Paquetes": OCULTA (`d-none`).
- SP nuevo `sp_nh_GetTratamientosPorPaciente` (UNION Cita+Consulta + JOIN Tratamiento → fecha, nombre, origen, comentarios).
- Endpoints/DTO nuevo solo lectura: `api/Historial/TratamientosPorPaciente/{pacienteId}` + wrappers MVC.

### Out of Scope
- Botones "Nuevo Tratamiento"/"Nueva Consulta" desde el historial (change 2).
- Capa PacientePaquete/PagoPaquete/TratamientoPaquete/CitaPaquete y Tab 4 (change 2 `paquetes-del-paciente`).
- Subida de archivos (módulo Anexo ya existe, no tocar).
- Dashboard. `Historial/Index` queda intacto.

## Capabilities

### New Capabilities
- `expediente-clinico`: vista de expediente de solo lectura (tabs + panel + grids) y endpoint `TratamientosPorPaciente` con su SP.

### Modified Capabilities
- None (el botón en el grid de Paciente es UI; no cambia requisitos a nivel spec).

## Approach

Seguir patrón MVC→WebApi→SP existente. Bootstrap 5 `nav-tabs` (ya cargado, sin patrón previo). Tratamientos se **derivan** de Cita+Consulta (no existe tabla de log; `TratamientoID` es FK nullable en `Cita`/`Consulta`). Drill-down reutiliza `Consulta/Receta/{id}` vía `window.open` (sin endpoint nuevo). Nombre completo del paciente se compone en el panel (API devuelve `Paciente` plano).

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `ConsultorioMedicoEntities/Catalogos/TratamientoHistoricoDTO.cs` | New | DTO (Id, Fecha, TratamientoNombre, Origen, Comentarios) + registro `.csproj` |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.Historial.cs` | New | `ObtenerTratamientosPorPaciente` → SP nuevo |
| `ConsultorioMedicoWebApi/Services/HistorialService.cs` | New | valida + llama DbWrapper |
| `ConsultorioMedicoWebApi/Controllers/HistorialController.cs` | New | `[Authorize] [RoutePrefix("api/Historial")]` → `TratamientosPorPaciente/{id}` |
| `ConsultorioMedicoMVC/DAL/HttpClientConnection.Historial.cs` | New | llamada `api/Historial/TratamientosPorPaciente/{id}` |
| `ConsultorioMedicoMVC/Services/HistorialService.cs` | New | servicio MVC |
| `ConsultorioMedicoMVC/Controllers/HistorialController.cs` | Modified | `Expediente(long id)` + `ConsultarTratamientosPorPaciente` |
| `ConsultorioMedicoMVC/Views/Historial/Expediente.cshtml` | New | tabs + panel + grids (Razor C# 5, BOM UTF-8) |
| `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml` | Modified | botón "Expediente Clínico" |
| `sql/sp_nh_Historial.sql` | New | `sp_nh_GetTratamientosPorPaciente` (solo `CREATE` + `IF OBJECT_ID`) |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| SP UNION devuelve duplicados/orden inconsistente | Med | `UNION` (dedup) + `ORDER BY Fecha DESC`; verificar contra BD real |
| `api/Paciente/{id}` sin `NombreCompleto` | Low | componer en el panel MVC |
| Vista `Receta.cshtml` server-side no sirve en modal | Low | abrir en nueva pestaña (`window.open`) |
| `GetAllConsultaByPacienteID` sin `TratamientoNombre` | — | cubierto por el SP UNION nuevo (no ALTERar SP existente) |

## Rollback Plan

Todo aditivo y de solo lectura. Revertir con `git checkout` de los archivos nuevos/modificados. El SP nuevo no se ejecuta en BD productiva salvo aprobación; se crea solo como script versionado (`sql/*.sql`), no DROP/ALTER. Sin migración de datos ni esquema.

## Dependencies

- BD productiva `DB_9C7990_Consultorio` con tablas `Cita`, `Consulta`, `Tratamiento` (FK `TratamientoID`).
- Bootstrap 5 bundle (ya en `_Layout.cshtml`).

## Success Criteria

- [ ] MSBuild solución Debug + `aspnet_compiler` precompilando vistas, exit 0.
- [ ] Botón "Expediente Clínico" abre `Historial/Expediente/{id}` con las 4 tabs (3 y 4 ocultas).
- [ ] Tab 1 muestra datos del paciente + grid de tratamientos (Cita+Consulta).
- [ ] Tab 2 doble clic abre `Consulta/Receta/{id}` en nueva pestaña.
- [ ] `api/Historial/TratamientosPorPaciente/{id}` responde `ModelResponse<T>` (HTTP 200, español).

## Nota (pregunta abierta → change 2)

¿El grid de tratamientos debe incluir también tratamientos provenientes de **paquetes**? Eso exige la capa `PacientePaquete`/`TratamientoPaquete` (change 2). En este change se deriva solo de Cita+Consulta; la inclusión de tratamientos de paquetes queda pendiente de confirmar en `paquetes-del-paciente`.
