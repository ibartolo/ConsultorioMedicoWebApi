# Archive Report — expediente-clinico-historial

**Change**: expediente-clinico-historial
**Fecha de archive**: 2026-10-27
**Modo**: openspec (file-based)
**Estado del verify**: PASS WITH WARNINGS (0 CRITICAL, 1 WARNING, 2 SUGGESTION)

---

## Resumen

Se archivó el change `expediente-clinico-historial`, la **BASE** del capability `expediente-clinico` (primero de una secuencia de archive). El change implementó la vista consolidada de **solo lectura** del expediente clínico: botón "Expediente Clínico" en el grid de Pacientes, vista `Historial/Expediente` con 4 pestañas Bootstrap 5 (Tab 3 "Archivos" y Tab 4 "Paquetes" ocultas), panel de datos del paciente, grid de tratamientos históricos derivados de Cita+Consulta (SP `sp_nh_GetTratamientosPorPaciente`), historial de consultas con drill-down a la receta, y endpoint de solo lectura `api/Historial/TratamientosPorPaciente/{pacienteId}` con su DTO `TratamientoHistoricoDTO`.

Verificación: build MSBuild exit 0 (tras restore de NuGet) y `aspnet_compiler` exit 0. 16/16 tareas completas. Sin hallazgos CRITICAL; el único WARNING es la tensión spec↔design "400 vs HTTP 200" resuelta deliberadamente por D7 (convención del proyecto: HTTP 200 siempre).

## Requisitos sincronizados

Se creó el spec principal `openspec/specs/expediente-clinico/spec.md` (no existía — este change es la base del capability). Se copió el delta spec completo (6 requirements, 9 scenarios):

| # | Requirement | Scenarios |
|---|-------------|-----------|
| 1 | Acceso al expediente desde el grid de Pacientes | 1 |
| 2 | Vista de expediente con pestañas | 2 |
| 3 | Panel de información del paciente | 1 |
| 4 | Grid de tratamientos históricos (Tab 1) | 2 |
| 5 | Historial de consultas (Tab 2) con drill-down | 1 |
| 6 | Endpoint de tratamientos por paciente | 3 |

## Estado del verify

- **Verdict**: PASS WITH WARNINGS (no bloquea el archive).
- **Build**: ✅ exit 0 (`MSBuild.exe DESI.ConsultorioMedico.sln /t:Build /p:Configuration=Debug`).
- **aspnet_compiler**: ✅ exit 0 (precompila `Expediente.cshtml` y `Paciente/Index.cshtml`).
- **Tareas**: 16/16 `[x]`.
- **Spec compliance**: 9/9 escenarios implementados; 1 con desviación deliberada (D7, "Id inválido" → `IsSuccess=false` HTTP 200 en vez de 400).
- **WARNING-1**: escenario "Id inválido → 400" no se cumple literalmente; intencional y correcto según D7 (misma tensión documentada en `dominio-paciente`). El spec principal conserva el texto "400" siguiendo la convención del proyecto (ver spec de `paciente`).
- **SUGGESTION-1/2**: botón solo-icono sin `aria-label`; SP vía `EXEC('...')` en vez de `CREATE PROCEDURE` plano. Opcionales.

## Contenido del archive

- proposal.md ✅
- exploration.md ✅
- design.md ✅
- tasks.md ✅ (16/16 completas)
- verify-report.md ✅
- specs/expediente-clinico/spec.md ✅

## Source of truth actualizado

- `openspec/specs/expediente-clinico/spec.md` (nuevo — requisitos del expediente clínico).

## Nota

- No se tocaron los otros changes activos (`expediente-acciones`, `expediente-ajustes`, `paquetes-lectura`, `calendario-citas-estados`, `paciente-modales-crud`, `paquetes-del-paciente`); se archivarán después en orden.
- Pregunta abierta hacia change 2 `paquetes-del-paciente`: incluir tratamientos provenientes de paquetes en el grid (requiere capa `PacientePaquete`/`TratamientoPaquete`).
