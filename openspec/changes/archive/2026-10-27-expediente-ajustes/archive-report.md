# Archive Report — expediente-ajustes

**Change**: expediente-ajustes
**Fecha de archive**: 2026-10-27
**Modo**: openspec (file-based)
**Verdict de verificación**: PASS WITH WARNINGS (0 CRITICAL)

## Resumen

Tercer change de la secuencia del expediente clínico (tras `expediente-clinico-historial` y `expediente-acciones`). Delta aplicado sobre `openspec/specs/expediente-clinico/spec.md`: **4 MODIFIED + 2 ADDED** requirements. El change se archivó en `openspec/changes/archive/2026-10-27-expediente-ajustes/`.

## Specs Sync (delta → main spec)

| Domain | Action | Detalle |
|--------|--------|---------|
| expediente-clinico | Updated | 4 MODIFIED + 2 ADDED requirements |

### MODIFIED (reemplazados en `openspec/specs/expediente-clinico/spec.md`)

1. **Grid de tratamientos históricos (Tab 1)** — grid derivado SOLO de Cita (antes Cita+Consulta). Origen constante "Cita". MUST NOT incluir tratamientos de Consulta.
2. **Endpoint de tratamientos por paciente** — `sp_nh_GetTratamientosPorPaciente` devuelve SOLO citas, MUST NOT hacer UNION con Consulta; DTO conserva `Id, Fecha, TratamientoNombre, Origen, Comentarios` con `Origen`="Cita" (contrato preservado). SP actualizado con patrón DROP+CREATE idempotente.
3. **Modal "Nuevo Tratamiento"** — Comentarios ahora `<textarea>` amplio (ancho completo, varias filas).
4. **Modal "Nueva Consulta"** — sin Descripción ni Receta; Comentarios ahora `<textarea>` amplio; guardado sigue funcionando (campos nullable).

### ADDED (anexados al final de la sección Requirements)

1. **Formato de fecha del grid de tratamientos** — columna Fecha en `dd/MM/yyyy hh:mm am/pm` (render JS manual, sin moment.js); fecha nula → celda vacía.
2. **Aislamiento del change** — MUST NOT modificar `Views/Consulta/Index.cshtml`, `Views/Cita/Index.cshtml` ni `Historial/Index`; MUST NOT crear endpoints/SPs/DTOs nuevos; Paquetes fuera de alcance.

### Coherencia adicional (mínima, no destructiva)

- Purpose: "historial de tratamientos (derivado de Cita+Consulta)" → "(derivado de Cita)".
- Nota (pregunta abierta → change 2): "se deriva solo de Cita+Consulta" → "solo de Cita".

## Archive Contents

- proposal.md ✅
- specs/expediente-clinico/spec.md ✅ (delta con 4 MODIFIED + 2 ADDED, 14 scenarios)
- design.md ✅ (decisiones D1–D5)
- tasks.md ✅ (11/13 `[x]`; 2 `[ ]` no bloqueantes — ver warnings)
- verify-report.md ✅ (PASS WITH WARNINGS, 0 CRITICAL)
- archive-report.md ✅ (este archivo)

## Warnings heredados (no bloqueantes)

- **WARNING-1**: escenario "Id inválido → 400" mantiene tensión spec↔design heredada de `expediente-clinico-historial`. La implementación devuelve HTTP 200 (`IsSuccess=false`) para `≤0` y 404 para no numérico (constraint `:long`), por convención del proyecto ("HTTP 200 siempre"). El texto "400" se conserva tal cual el delta; el endpoint no fue modificado por este change.
- **WARNING-2**: tarea 5.1 marcada `[ ]` (BLOQUEADO) pero el build SÍ pasa (exit 0) vía VsDevCmd (solo el MSBuild directo sin entorno VS falla por falta de `NuGet.targets`).
- **SUGGESTION-1**: comentario obsoleto en `DbWrapper.Historial.cs:14` ("Cita + Consulta" → "solo Cita").
- **SUGGESTION-2**: verificación en navegador pendiente (tarea 5.5), verificada estáticamente + aspnet_compiler + BD.

## Estado final verificado

- Grid de tratamientos: solo Citas (sin UNION con Consulta). ✅
- Modal "Nueva Consulta": sin Descripción/Receta; Comentarios `<textarea>` amplio. ✅
- Modal "Nuevo Tratamiento": Comentarios `<textarea>` amplio. ✅
- Formato fecha `dd/MM/yyyy hh:mm am/pm`. ✅
- Changes activos restantes (`paquetes-lectura`, `calendario-citas-estados`, `paciente-modales-crud`, `paquetes-del-paciente`) NO tocados. ✅
