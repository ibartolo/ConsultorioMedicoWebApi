# Proposal: Paciente — CRUD en modales + baja lógica

## Intent

Migrar el alta/edición de Paciente de página full-page (`Nuevo.cshtml`) a un **modal ancho y largo** reutilizable, y añadir **baja lógica (Eliminar)** hoy inexistente. Todo con actualización de la fila afectada en DataTables (sin recargar la tabla) y confirmaciones con SweetAlert2.

## Scope

### In Scope
- Partial `_Formulario.cshtml` (form de Paciente) reutilizado por el modal (alta Id=0 y edición Id>0).
- Modal `modal-xl` en `Index.cshtml`; el botón "Nuevo Paciente" y el botón Editar de cada fila abren el modal.
- Cargar datos de edición vía nueva acción MVC JSON `ConsultarPacientePorId(long)` (opción **b**).
- Guardar con validación (jQuery Validate) + confirmación SweetAlert → POST existente `GuardarOActualizarPaciente` (serialize form-data).
- Refrescar SOLO la fila afectada (`table.row(tr).data(dto).invalidate().draw(false)`); en alta nueva, insertar la fila nueva.
- Eliminar (baja lógica `Estatus=0`): SP + WebApi + MVC; al eliminar, `table.row(tr).remove().draw(false)`.

### Out of Scope
- Expediente/Historial, Cita, Consulta y resto del WebApi.
- Baja física de Paciente.
- No romper rutas existentes (`Paciente/Nuevo/{id}` se conserva como wrapper).

## Capabilities

### New Capabilities
<!-- Ninguna: la baja lógica y el flujo modal pertenecen al dominio existente `paciente`. -->

### Modified Capabilities
- `paciente`: añade requisito de baja lógica (Eliminar) y cambia el flujo UI de alta/edición de full-page a modal.

## Approach

1. Convertir `Views/Paciente/Nuevo.cshtml` → partial `_Formulario.cshtml` (mismo `#pacienteForm`, `@model Paciente`, hidden Id/Estatus/auditoría; JS de guardado adaptado a callback en vez de `window.location`).
2. Modal en `Index.cshtml`; Editar llama `ConsultarPacientePorId(id)` y rellena el form con los datos frescos.
3. Guardar → validar → confirmar → `GuardarOActualizarPaciente`; en éxito obtener el Id (`ModelResponse<int>`) y re-fetch para poblar/insertar la fila, cerrar modal.
4. Eliminar replica el patrón existente de Cita (`sp_nh_DeleteCita`): SP → DbWrapper → WebApi `[HttpPost, Route("Eliminar/{id:int}")]` → MVC `[HttpPost] EliminarPaciente` → JSON.

## Affected Areas

| Area | Impacto | Descripción |
|------|---------|-------------|
| `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml` | Modified | Modal + botones Editar/Eliminar + JS refresh de fila |
| `ConsultorioMedicoMVC/Views/Paciente/_Formulario.cshtml` | New | Partial extraída de Nuevo |
| `ConsultorioMedicoMVC/Views/Paciente/Nuevo.cshtml` | Modified | Wrapper que renderiza el partial |
| `ConsultorioMedicoMVC/Controllers/PacienteController.cs` | Modified | `ConsultarPacientePorId`, `[HttpPost] EliminarPaciente` |
| `ConsultorioMedicoMVC/Services/PacienteService.cs` | Modified | Métodos consultar/eliminar |
| `ConsultorioMedicoMVC/DAL/HttpClientConnection.Paciente.cs` | Modified | `ConsultarPacientePorId`, `EliminarPaciente` |
| `ConsultorioMedicoWebApi/Controllers/PacienteController.cs` | Modified | `[HttpPost, Route("Eliminar/{id:int}")]` |
| `ConsultorioMedicoWebApi/Services/PacienteService.cs` | Modified | `EliminarPaciente` |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.Paciente.cs` | Modified | `EliminarPaciente(int)` → ExecuteNonQuery |
| `sql/sp_nh_Paciente.sql` | Modified | `sp_nh_DeletePaciente` (CREATE + IF OBJECT_ID) |
| `ConsultorioMedicoMVC/ConsultorioMedicoMVC.csproj` | Modified | Registrar `_Formulario.cshtml` como `<Content>` |

## Risks

| Riesgo | Prob | Mitigación |
|--------|------|-----------|
| SP no ejecutado en BD productiva | High | Incluir `sp_nh_DeletePaciente` en el script y ejecutarlo antes de desplegar |
| Redirect vs refresh de fila rompe el flujo | Med | Callback JS por modal, sin `window.location` |
| DTO de fila desactualizado (NombreCompleto/Edad) | Med | Opción (b): re-fetch por Id |
| `row()` desincronizado tras ordenar/filtrar | Low | Capturar referencia `tr` al abrir el modal |

## Rollback Plan

- Revertir commits MVC/WebApi (los endpoints nuevos son aditivos, no rompen rutas existentes).
- SP: `IF OBJECT_ID('sp_nh_DeletePaciente') IS NOT NULL DROP PROCEDURE sp_nh_DeletePaciente` (no destructivo si no se invoca).
- `Nuevo.cshtml` se conserva como fallback full-page hasta validar el modal.

## Dependencies

- Ejecutar `sp_nh_DeletePaciente` en BD productiva (Smarterasp) antes del despliegue.

## Notas y Decisiones

- **Punto 2 — carga de datos para edición: opción (b)** `ConsultarPacientePorId(long)` vía `ObtenerPacientePorId`. Justificación: incluye `NombreCompleto`/`Edad` recalculados y no depende del DTO de la fila (datos frescos, evita desincronización).
- **Punto 4 — DTO actualizado tras guardar:** re-fetch por el Id retornado (`ModelResponse<int>` de `GuardarOActualizarPaciente`) para poblar/insertar la fila con datos consistentes.
- **`Nuevo.cshtml`:** se mantiene como wrapper que renderiza el partial (compatibilidad de ruta `Paciente/Nuevo/{id}`), no se elimina del flujo.
- **Eliminar:** botón por fila (no dentro del modal), siguiendo el patrón de acciones por fila ya usado en la columna Acciones.

## Success Criteria

- [ ] Alta y edición desde el modal funcionan con validación y confirmación.
- [ ] Eliminar realiza baja lógica y quita la fila sin recargar la tabla.
- [ ] Guardar actualiza/inserta SOLO la fila afectada.
- [ ] MSBuild + aspnet_compiler exit 0; rutas existentes intactas.
