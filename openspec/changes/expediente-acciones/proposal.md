# Proposal: Acciones del Expediente (Nuevo Tratamiento / Nueva Consulta + fix de ancho)

## Intent

Tras probar `expediente-clinico-historial`, el usuario pide: (1) que las dos DataTables (tratamientos Tab 1, consultas Tab 2) ocupen SIEMPRE el 100% del ancho, alineadas con "Volver a Pacientes" (hoy usan `responsive:true` y se encogen); (2) dos botones en el header — "Nuevo Tratamiento" (crea una **Cita** con paciente fijo) y "Nueva Consulta" — replicando la WinForms, recargando solo el grid afectado al guardar. NO se implementa validación de choque de horarios (la WinForms no la tenía; evidencia §5).

## Scope

### In Scope
- Fix visual: quitar `responsive:true` de ambas DataTables y forzar `width:100%`.
- Botones header alineados: "Nuevo Tratamiento", "Nueva Consulta", "Volver a Pacientes".
- Modal de Cita (paciente fijo) → POST `CitaController.GuardarOActualizarCita` → `sp_nh_SaveCita`; al guardar recarga `#tratamientosTable`.
- Modal de Consulta (paciente fijo) → POST `ConsultaController.GuardarOActualizarConsulta` → `sp_nh_SaveConsulta`; al guardar recarga `#consultasTable`.
- SweetAlert éxito/error + recarga SOLO del grid afectado.

### Out of Scope
- Validación de disponibilidad/choque de horarios (no estaba en la WinForms; §5).
- Tabla nueva de tratamientos (NO existe; se derivan de Cita+Consulta).
- Pestañas ocultas Archivos/Paquetes (change `paquetes-del-paciente`).
- `Historial/Index` y `Cita/Index`/`Consulta/Index` intactos.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `expediente-clinico`: agrega acciones de creación desde el header (Nuevo Tratamiento→Cita, Nueva Consulta→Consulta) y el requisito de ancho 100% de los grids. (Capability existe como delta en `expediente-clinico-historial`, aún no archivada a `openspec/specs/`.)

## Approach

**Decisión de reutilización — Opción (a): copiar los modales dentro de `Expediente.cshtml` con paciente fijo.** Se replica el markup+JS de `#citaForm` y `#consultaForm`, pero el `<select PacienteID>` se sustituye por `<input type="hidden" name="PacienteID" value="@Model.Id">` + nombre del paciente (solo lectura). Se conservan los dropdowns Tratamiento/TipoConsulta/Estado y jQuery Validate; los POST reutilizan los endpoints existentes (sin endpoint/SP/DTO nuevos). El callback de éxito cambia `calendar.refetchEvents()` por `tratamientosTable.ajax.reload()` / `consultasTable.ajax.reload()`.

Se descartan: (b) parametrizar las vistas existentes — obligaría a navegar fuera del expediente (rompe el UX de modal) y añade refactor con riesgo de regresión; (c) extraer partials — patrón sin precedente en el proyecto. La opción (a) es la más simple y consistente con el patrón de vistas autocontenidas.

`HistorialController.Expediente(id)` debe además cargar `ViewBag.Tratamientos` (reusando `_citaService.ConsultarTratamientos()`) y `ViewBag.TiposConsulta` (agregar `_tipoConsultaService`) para poblar los dropdowns de los modales.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `ConsultorioMedicoMVC/Views/Historial/Expediente.cshtml` | Modified | Quitar `responsive`; forzar ancho; botones header; 2 modales + JS; callback recarga grid |
| `ConsultorioMedicoMVC/Controllers/HistorialController.cs` | Modified | `Expediente`: cargar `ViewBag.Tratamientos` y `ViewBag.TiposConsulta` |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Duplicación de markup/JS de modales | Med | Aceptado por simplicidad; extraer partial en change futuro si crece |
| jQuery Validate ignora `<input hidden>` con `required` | Low | Paciente fijo siempre con valor; quitar regla `PacienteID required` si interfiere |
| DataTable aún no ocupe 100% tras quitar `responsive` | Low | Forzar `width:100%` explícito; verificar en navegador |

## Rollback Plan

Aditivo sobre una vista y un controller. Revertir con `git checkout` de `Expediente.cshtml` y `HistorialController.cs`. Sin cambios en SPs, endpoints, DTOs ni BD; sin migración de datos.

## Dependencies

- Endpoints existentes `CitaController.GuardarOActualizarCita`, `ConsultaController.GuardarOActualizarConsulta` + SPs `sp_nh_SaveCita`, `sp_nh_SaveConsulta`.
- Servicios `CitaService.ConsultarTratamientos`, `TipoConsultaService.ConsultarTiposConsulta`.

## Success Criteria

- [ ] MSBuild Debug + aspnet_compiler exit 0.
- [ ] Ambas DataTables ocupan 100% y se alinean con el header.
- [ ] "Nuevo Tratamiento" abre modal de Cita con paciente fijo; guardar recarga Tab 1 + SweetAlert.
- [ ] "Nueva Consulta" abre modal de Consulta con paciente fijo; guardar recarga Tab 2 + SweetAlert.
- [ ] Sin validación de choque de horarios; `Historial/Index` y pestañas ocultas intactas.
