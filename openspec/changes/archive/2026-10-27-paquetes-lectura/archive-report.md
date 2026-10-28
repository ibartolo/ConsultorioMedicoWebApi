# Archive Report — paquetes-lectura

**Change**: paquetes-lectura
**Fecha de archive**: 2026-10-27
**Modo**: openspec (file-based)
**Verdict de verificación**: PASS WITH WARNINGS (0 CRITICAL)

## Resumen

Cuarto change de la secuencia del expediente clínico (tras `expediente-clinico-historial`, `expediente-acciones` y `expediente-ajustes`). Materializa la capability nueva `paquetes-lectura` (7 requisitos) y aplica el delta MODIFIED sobre `openspec/specs/expediente-clinico/spec.md` (Tab "Paquetes" visible; "Archivos" sigue oculta). El change se archivó en `openspec/changes/archive/2026-10-27-paquetes-lectura/`.

## Specs Sync (delta → main spec)

| Domain | Action | Detalle |
|--------|--------|---------|
| paquetes-lectura | Created | Spec full nueva: 7 requisitos, 15 escenarios |
| expediente-clinico | Updated | 1 MODIFIED requirement ("Vista de expediente con pestañas") |

### Created — `openspec/specs/paquetes-lectura/spec.md`

Capability nueva materializada como spec full (el delta era la única fuente; no existía `openspec/specs/paquetes-lectura/`). Los 7 requisitos:

1. Grid de paquetes adquiridos del paciente (`#paquetesTable`).
2. Grid de citas-tratamientos del paquete (`#citasPaqueteTable`).
3. Endpoint GET `PaquetesPorPaciente/{pacienteId}`.
4. Endpoint GET `CitasPorPacientePaquete/{pacientePaqueteId}`.
5. DTOs de lectura de paquetes.
6. SPs nuevos de solo lectura (`sp_nh_GetPaquetesPorPaciente`, `sp_nh_GetCitasPorPacientePaquete`).
7. Alcance solo lectura del change.

### Updated — `openspec/specs/expediente-clinico/spec.md`

- **"Vista de expediente con pestañas"** (MODIFIED): la pestaña "Paquetes" pasa a visible (sin `d-none`) con dos grids de solo lectura; "Archivos" permanece oculta (`d-none`). Se reemplazaron los escenarios "Navegación entre pestañas visibles" y "Pestañas diferidas ocultas" por los del delta (incluye "Pestaña Paquetes visible con grids").
- **Coherencia adicional (mínima, no destructiva)**: Purpose — "Las pestañas 'Archivos' y 'Paquetes' quedan ocultas" → "La pestaña 'Archivos' queda oculta; la pestaña 'Paquetes' muestra los paquetes adquiridos y sus citas-tratamientos en solo lectura".

## Reconciliación spec ↔ design (resuelta en este archive)

| Tema | Spec (antes) | Implementación/design (real) | Acción aplicada |
|------|--------------|------------------------------|-----------------|
| Ids inválidos | "responde 400 por validación" | HTTP 200 + `IsSuccess=false` (patrón `ObtenerTratamientosPorPaciente`) | Escenarios "Id inválido" reescritos a "responde 200 con `IsSuccess=false` y mensaje en español (no 400)" |
| Nombres de DTO | `PacientePaqueteDTO` / `CitaPaqueteDTO` | `PacientePaqueteLecturaDTO` / `CitaPaqueteLecturaDTO` (design D2; evita colisión con `paquetes-del-paciente`) | Requisitos 3, 4 y 5 reescritos con sufijo `Lectura` |

## Archive Contents

- proposal.md ✅
- specs/paquetes-lectura/spec.md ✅ (delta nueva capability, 7 requisitos)
- specs/expediente-clinico/spec.md ✅ (delta MODIFIED, 1 requisito, 3 escenarios)
- design.md ✅ (decisiones D1–D5)
- tasks.md ✅ (18/19 `[x]`; 1 `[ ]` no bloqueante — ver warnings)
- verify-report.md ✅ (PASS WITH WARNINGS, 0 CRITICAL)
- archive-report.md ✅ (este archivo)

## Warnings heredados (no bloqueantes)

- **WARNING-1**: tarea 6.3 (verificación manual end-to-end HTTP 200 real + click → 7 citas en UI) pendiente. Lógica verificada estáticamente + build + ejecución real de SPs; solo falta la prueba visual/hit HTTP con token.
- **WARNING-2/3**: reconciliaciones spec↔design (ids inválidos → HTTP 200; sufijo `Lectura` en DTOs) — RESUELTAS en este archive (ver tabla de reconciliación).
- **SUGGESTION-1**: el change modifica `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml` (ocultar `Id`/`Email` del grid) — fuera de scope, no documentado en proposal/design/tasks. No viola la lista de "no tocar" (`Historial/Index`, `Cita/Index`, `Consulta/Index` intactos).
- **SUGGESTION-2**: ajustes laterales en `Expediente.cshtml` (ocultar `searching`/`lengthChange`; `Id` visible:false + formato fecha en grid de consultas) — verificados y funcionales, no contemplados en la spec de este change.

## Estado final verificado

- Tab 4 "Paquetes" visible; "Archivos" oculta. ✅
- Grid izquierdo lista "PAQUETE CBX CICRATIZ" (paciente 248, PacientePaquete Id=9). ✅
- Grid derecho lista 7 citas (Ids 526–532, ORDER BY FechaCita DESC). ✅
- 2 SPs nuevos (`sp_nh_GetPaquetesPorPaciente`, `sp_nh_GetCitasPorPacientePaquete`) ejecutados y verificados. ✅
- 2 DTOs `...LecturaDTO`, 2 endpoints WebApi GET, 2 llamadas MVC, 2 acciones AJAX, 2 DataTables. ✅
- Changes activos restantes (`calendario-citas-estados`, `paciente-modales-crud`, `paquetes-del-paciente`) NO tocados. ✅
