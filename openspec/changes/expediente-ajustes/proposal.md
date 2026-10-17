# Proposal: Ajustes al Expediente Clínico

## Intent

Tras probar el expediente, el usuario pide 4 ajustes: (1) corregir el bug del grid "Tratamientos Históricos" —hoy mezcla Citas + Consultas vía `UNION`; la WinForms original ("Historial de Tratamientos" = dgvCita) mostraba SOLO citas—; (2) formatear la fecha del grid; (3) ampliar el campo Comentarios del modal "Nuevo Tratamiento"; (4) quitar Descripción/Receta del modal "Nueva Consulta" del expediente.

## Scope

### In Scope
- SP `sp_nh_GetTratamientosPorPaciente` devuelve SOLO Citas (quitar el `UNION` con `Consulta`).
- Columna Fecha del grid de tratamientos con formato `dd/MM/yyyy hh:mm am/pm` (render JS manual, sin moment.js).
- Modal "Nuevo Tratamiento": Comentarios → `<textarea>` amplia (varias filas, ancho completo).
- Modal "Nueva Consulta" (solo del Expediente): quitar Descripción y Receta; Comentarios → `<textarea>` amplia.

### Out of Scope
- `Views/Consulta/Index.cshtml`, `Views/Cita/Index.cshtml`, `Historial/Index` (no se tocan).
- Pestaña Paquetes (change `paquetes-del-paciente`).
- Endpoints/DTO/SP nuevos.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `expediente-clinico`: cambia el requisito del grid de tratamientos (solo Citas), el formato de fecha, y la composición de los modales del expediente. (Capability vivo como delta en `expediente-clinico-historial` / `expediente-acciones`, aún no archivado a `openspec/specs/`.)

## Approach

1. **SP solo-Citas**: en `sp_nh_GetTratamientosPorPaciente` conservar únicamente la rama `Cita` (quitar el `UNION` con `Consulta`). Mantener `SELECT c.Id, c.FechaCita AS Fecha, t.Nombre AS TratamientoNombre, 'Cita' AS Origen, c.Comentarios ... ORDER BY Fecha DESC`.
   - **Criterio de actualización (objeto ya en BD)**: el script actual usa `IF OBJECT_ID IS NULL` (solo crea si no existe), por lo que no se re-ejecuta. Como `sp_nh_` es objeto propio de la migración (SELECT-only, sin datos, única dependencia `DbWrapper.Historial.cs`), actualizarlo es seguro. Se reescribe el script con patrón idempotente `IF OBJECT_ID(...) IS NOT NULL DROP PROCEDURE ...; GO; CREATE PROCEDURE ...` (DROP+CREATE guardado), coherente con la convención pero adaptado a un objeto ya existente.
2. **Fecha**: columna `Fecha` de `#tratamientosTable` con `render` JS manual → `dd/MM/yyyy hh:mm am/pm` (sin moment.js; parseo defensivo de string ISO o `/Date()`).
3. **Textareas**: Comentarios de ambos modales → `<textarea rows="4" class="form-control w-100">`.
4. **Quitar campos**: eliminar bloques Descripción (`name="Descripcion"`) y Receta (`name="DescripcionReceta"`) de `#modalConsultaExpediente`. Son nullable en `sp_nh_SaveConsulta`; al no serializarse quedan null.

**Origen**: se mantiene el campo `Origen` (`'Cita'` constante) en SP/DTO/mapeo para no romper el contrato del endpoint; la columna seguirá mostrando "Cita". (Alternativa: quitarla — descartada por ampliar la superficie de cambio sin ganancia funcional.)

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `sql/sp_nh_Historial.sql` | Modified | SP solo-Citas; patrón DROP+CREATE idempotente |
| `ConsultorioMedicoMVC/Views/Historial/Expediente.cshtml` | Modified | render fecha, textareas, quitar Descripción/Receta |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.Historial.cs` | Unchanged | solo cambiaría si se quita `Origen` (no recomendado) |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| DROP del SP en BD productiva cause regresión | Low | objeto propio SELECT-only; ejecutar solo con aprobación, DROP+CREATE guardado |
| Quitar Descripción/Receta rompa el POST | Low | campos nullable; la validación no los exige |
| Formato de fecha dependa del formato JSON recibido | Med | render defensivo (ISO o `/Date()`); verificar en navegador |

## Rollback Plan

- Vista/JS: `git checkout` de `Expediente.cshtml`.
- SP: re-ejecutar la versión previa (UNION Cita+Consulta) con el mismo DROP+CREATE idempotente. Sin migración de datos.

## Dependencies

- BD productiva `DB_9C7990_Consultorio` (SP ya ejecutado). Bootstrap 5 + DataTables 1.13.6 (sin moment.js).

## Success Criteria

- [ ] MSBuild Debug + aspnet_compiler exit 0.
- [ ] Paciente 19803 (1 consulta, 0 citas): grid de tratamientos VACÍO; la consulta solo en "Historial de consultas".
- [ ] Fecha en grid = `dd/MM/yyyy hh:mm am/pm`.
- [ ] Comentarios de ambos modales son textareas amplias.
- [ ] Modal "Nueva Consulta" sin Descripción ni Receta; el de `Consulta/Index` intacto.
