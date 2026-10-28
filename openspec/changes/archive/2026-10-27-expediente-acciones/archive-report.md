# Archive Report: `expediente-acciones`

**Fecha**: 2026-10-27
**Change**: expediente-acciones
**Estado previo al archivo**: implementado y verificado (PASS WITH WARNINGS)
**Artifact store**: openspec (file-based)
**Posición en secuencia**: SEGUNDO change archivado (precedido por `expediente-clinico-historial`)

---

## Specs Sync

| Domain | Action | Details |
|--------|--------|---------|
| `expediente-clinico` | Updated | 6 requisitos ADDED, 0 modified, 0 removed |

Los 6 requisitos ADDED se agregaron a `openspec/specs/expediente-clinico/spec.md` (a la sección Requirements, tras `Endpoint de tratamientos por paciente` y antes de la `## Nota`), sin duplicar ni borrar los requisitos previos de `expediente-clinico-historial`.

### Requisitos agregados

1. **Ancho 100% de las DataTables del expediente** — quitar `responsive:true`; `width:100%`; `columns.adjust()` al cargar y en `shown.bs.tab`.
2. **Botones de acción en el header del expediente** — "Nuevo Tratamiento", "Nueva Consulta", "Volver a Pacientes".
3. **Modal "Nuevo Tratamiento" (crea una Cita con paciente fijo)** — POST `CitaController.GuardarOActualizarCita` → `sp_nh_SaveCita`; recarga solo `#tratamientosTable`.
4. **Modal "Nueva Consulta" (crea una Consulta con paciente fijo)** — POST `ConsultaController.GuardarOActualizarConsulta` → `sp_nh_SaveConsulta`; recarga solo `#consultasTable`.
5. **Validación de los modales del expediente** — fecha requerida; Tratamiento/TipoConsulta opcionales; sin validación de choque de horarios.
6. **Reutilización de endpoints y contratos existentes** — sin endpoints/SPs/DTOs nuevos.

### Nota de estado futuro

`expediente-ajustes` (change activo) MODIFICARÁ posteriormente algunos de estos requisitos (por ejemplo, la remoción de `Descripcion`/`DescripcionReceta` del modal Consulta). Se respetó el estado ACTUAL de los requisitos tal como los define el delta de `expediente-acciones`; el merge de `expediente-ajustes` aplicará esos ajustes sobre esta base.

---

## Archive Contents

- `proposal.md` ✅
- `specs/expediente-clinico/spec.md` ✅
- `design.md` ✅
- `tasks.md` ✅ (11/12 tareas completas; 6.3 = verificación manual pendiente, no automatizable)
- `verify-report.md` ✅ (PASS WITH WARNINGS)
- `archive-report.md` ✅ (este archivo)

## Verificación del Archive

- [x] Main spec `openspec/specs/expediente-clinico/spec.md` actualizado con los 6 requisitos ADDED
- [x] Change folder movido a `openspec/changes/archive/2026-10-27-expediente-acciones/`
- [x] Archive contiene todos los artefactos (proposal, specs, design, tasks, verify-report)
- [x] `openspec/changes/` ya no contiene `expediente-acciones`
- [x] Changes activos restantes intactos (`expediente-ajustes`, `paquetes-lectura`, `calendario-citas-estados`, `paciente-modales-crud`, `paquetes-del-paciente`)

## Issues Heredados (no bloqueantes)

- Verificación manual en navegador (tarea 6.3) queda pendiente — requiere sesión autenticada.
- Diferencia semántica spec vs. diseño en "Validación de los modales" (spec dice "PacienteID y fecha son requeridos"; el código omite `PacienteID required` por decisión D3 — el hidden siempre tiene `@Model.Id`).
- Diferencias `Descripcion`/`DescripcionReceta` se resolverán en `expediente-ajustes`.
