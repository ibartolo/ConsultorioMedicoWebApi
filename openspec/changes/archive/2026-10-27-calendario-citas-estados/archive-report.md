# Archive Report

**Change**: calendario-citas-estados
**Fecha de archivo**: 2026-10-27
**Veredicto de verificación**: PASS WITH WARNINGS (sin CRITICAL)
**Artifact store**: openspec (file-based)

---

## Resumen

Change archivado tras implementación y verificación. Se sincronizó la capability nueva `cita` (5 requisitos) en `openspec/specs/cita/spec.md` y se movió la carpeta del change a `openspec/changes/archive/2026-10-27-calendario-citas-estados/`.

## Specs Sincronizadas

| Domain | Action | Detalles |
|--------|--------|----------|
| cita | Creada | 5 requisitos ADDED (0 modificados, 0 removidos). No existía spec previa de `cita` en `openspec/specs/`. |

Requisitos en `openspec/specs/cita/spec.md`:
1. Colores de eventos según estado (1 #FD7E14 / 2 #198754 / 3 #DC3545 / null #6C757D).
2. Tooltip con estado del evento (eventDidMount; "Sin estado" para null).
3. Baja lógica de Cita vía SP `sp_nh_DeleteCita` (UPDATE Estatus=0, idempotente).
4. Endpoint de eliminación `POST api/Cita/Eliminar/{id:int}` → `ModelResponse<bool>`.
5. Acción MVC y botón Eliminar (visible solo `Id>0`, SweetAlert, refetchEvents).

## Reconciliaciones aplicadas a la spec

| Ítem | Delta (spec original) | Main spec reconciliada |
|------|-----------------------|------------------------|
| Endpoint | `POST api/Cita/Eliminar` | `POST api/Cita/Eliminar/{id:int}` (id por ruta, patrón del controller; design D5) |
| Id inválido | no explicitado | `POST .../{id:int}` con id ≤ 0 o no numérico → HTTP 200 + `IsSuccess=false` (no 400); se añadió scenario "Id inválido" |
| Tipo de respuesta | `ModelResponse<bool>` (spec ya era fuente de verdad) | se conserva `ModelResponse<bool>` (no `int` del proposal) |
| Tooltip default | "Sin estado" | se conserva (reconciliado en design D3) |

## Contenido del Archive

- proposal.md ✅
- specs/cita/spec.md ✅ (delta)
- design.md ✅
- tasks.md ✅ (17/18 completas; `6.4` E2E manual pendiente)
- verify-report.md ✅ (PASS WITH WARNINGS)
- archive-report.md ✅

## Source of Truth Actualizado

- `openspec/specs/cita/spec.md` (creado)

## Notas / Warnings (no bloqueantes)

- `6.4` E2E manual no ejecutado (sin harness de browser); verificado por evidencia estática + build + aspnet_compiler.
- `sql/sp_nh_Historial.sql` usa DROP+CREATE (histórico), no "CREATE idempotente" como describe proposal/spec; es operación fuera de la capability `cita`.
- Endpoint real es route param `Eliminar/{id:int}` vs literal `Eliminar` (reconciliado en D5 y en la spec).
