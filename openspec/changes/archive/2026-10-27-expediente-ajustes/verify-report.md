# Verification Report — expediente-ajustes

**Change**: expediente-ajustes
**Fecha**: 2026-10-16
**Modo**: Standard (strict_tdd = false)
**Versión spec**: 4 MODIFIED + 2 ADDED requirements, 14 scenarios

---

## Resumen ejecutivo

La implementación de los ajustes al expediente clínico está **completa y funcionalmente alineada** con el spec y el design. El cambio es quirúrgico (solo `sql/sp_nh_Historial.sql` + `ConsultorioMedicoMVC/Views/Historial/Expediente.cshtml`): el SP devuelve ahora **SOLO Cita** (sin UNION), la columna Fecha del grid se formatea con JS manual `dd/MM/yyyy hh:mm am/pm`, y los modales autocontenidos tienen `Comentarios` como `<textarea rows="4">` (el modal Nueva Consulta ya no incluye Descripción ni Receta). Validación directa contra BD productiva: `@IdPaciente=19803` → **0 filas** y `@IdPaciente=3627` → **2 filas, ambas Origen='Cita'**. El build compila (exit 0 vía VsDevCmd) y `aspnet_compiler` precompila las vistas (exit 0). `Consulta/Index.cshtml` conserva Descripción/Receta, y `Cita/Index.cshtml`/`Historial/Index.cshtml` permanecen intactos.

Se detectan **0 hallazgos CRITICAL**, **2 WARNING** y **2 SUGGESTION**. Ninguno bloquea el archive.

---

## Completeness (Tareas)

| Métrica | Valor |
|---------|-------|
| Tareas totales | 13 (1.1–5.5) |
| Completadas `[x]` | 11 |
| Incompletas `[ ]` | 2 (5.1, 5.5) |

Incompletas:
- **5.1** Build `MSBuild ... /t:Build` — marcada `[ ]` con nota "BLOQUEADO por defecto de entorno". **Verificado en esta sesión: el build SÍ pasa (exit 0) usando el entorno VsDevCmd** (ver Build & Tests abajo). La marca `[ ]` es imprecisa: el fallo es solo del MSBuild directo sin entorno VS (falta `NuGet.targets`), no del change. Ver WARNING-2.
- **5.5** Navegador — no ejecutado (entorno headless). Cambios verificados estáticamente + aspnet_compiler. Ver SUGGESTION-2.

Todas las tareas `[x]` fueron verificadas contra el código real y la BD (no solo los `.md`).

---

## Build & Tests (Ejecución real)

**Build**: ✅ Passed (exit 0) — vía VsDevCmd
```
Versión de MSBuild 17.14.51+25f168cee para .NET Framework
  ConsultorioMedicoEntities -> ...\ConsultorioMedicoEntities.dll
  ConsultorioMedicoMVC     -> ...\ConsultorioMedicoMVC.dll
  ConsultorioMedicoWebApi  -> ...\ConsultorioMedicoWebApi.dll
```
Comando: `VsDevCmd.bat -arch=amd64 && MSBuild DESI.ConsultorioMedico.sln /t:Restore && MSBuild ... /t:Build /p:Configuration=Debug`
Sin errores (solo warning pre-existente CS1998 en `Startup.cs`).

**Tests**: ➖ No aplica — no hay test runner (sin xUnit, strict_tdd = false). Verificación = MSBuild + aspnet_compiler.

**aspnet_compiler (precompilación de vistas)**: ✅ Passed (exit 0)
```
Herramienta de compilación de Microsoft (R) ASP.NET versión 4.8.9037.0
```
Comando: `aspnet_compiler.exe -p "ConsultorioMedicoMVC" -v / "<temp>\mvc_check"` (valida Razor C# 5 de `Expediente.cshtml`).

**Coverage**: ➖ No disponible (`coverage.available: false`).

---

## Spec Compliance Matrix

> Modo Standard sin test runner: la evidencia es **estructural** (código real) + **compilación** (MSBuild + aspnet_compiler) + **validación de BD** (SQLCMD solo-lectura). No hay tests automatizados en este proyecto.

| # | Requirement | Scenario | Evidencia | Resultado |
|---|-------------|----------|-----------|-----------|
| 1 | Grid tratamientos (Tab 1) | Tratamientos listados (solo citas) | `sql/sp_nh_Historial.sql:26-31` — single SELECT `FROM dbo.Cita` (sin UNION), `'Cita' AS Origen`, `ORDER BY c.FechaCita DESC`. Confirmado en BD: `UNION_present=0`, `FROM_Consulta_present=0` | ✅ COMPLIANT |
| 1 | Grid tratamientos (Tab 1) | Paciente con consultas y sin citas | BD `EXEC ... @IdPaciente=19803` → **0 filas**; `dataSrc` retorna `[]` sin error (`Expediente.cshtml:277`) | ✅ COMPLIANT |
| 2 | Endpoint tratamientos | Respuesta exitosa | `HistorialController.cs:20-22` intacto → `ModelResponse<List<TratamientoHistoricoDTO>>`; BD `@3627` → 2 filas | ✅ COMPLIANT |
| 2 | Endpoint tratamientos | Paciente con consultas y sin citas | BD `@19803` → 0 filas → `IsSuccess=true`, `Response=[]` | ✅ COMPLIANT |
| 2 | Endpoint tratamientos | Id inválido | `{pacienteId:long}` + guard `<=0` → `IsSuccess=false` HTTP 200 (no 400). Endpoint NO modificado por este change | ⚠️ PARTIAL (ver WARNING-1) |
| 3 | Modal "Nuevo Tratamiento" | Campo Comentarios amplio | `Expediente.cshtml:161` — `<textarea class="form-control" name="Comentarios" rows="4" ...>` | ✅ COMPLIANT |
| 3 | Modal "Nuevo Tratamiento" | Creación exitosa | `Expediente.cshtml:332-354` — POST `Cita/GuardarOActualizarCita`, SweetAlert éxito, recarga solo `tratamientosTable.ajax.reload()` | ✅ COMPLIANT |
| 3 | Modal "Nuevo Tratamiento" | Guardado fallido | `Expediente.cshtml:347-352` — SweetAlert error, modal permanece abierto | ✅ COMPLIANT |
| 4 | Modal "Nueva Consulta" | Modal sin Descripción ni Receta | `Expediente.cshtml:183-229` — sin `name="Descripcion"` ni `name="DescripcionReceta"`; `Comentarios` `rows="4"` (línea 226) | ✅ COMPLIANT |
| 4 | Modal "Nueva Consulta" | Creación exitosa | `Expediente.cshtml:361-383` — POST `Consulta/GuardarOActualizarConsulta`, SweetAlert éxito, recarga solo `consultasTable.ajax.reload()` | ✅ COMPLIANT |
| 4 | Modal "Nueva Consulta" | Guardado fallido | `Expediente.cshtml:375-381` — SweetAlert error, modal permanece abierto | ✅ COMPLIANT |
| 5 | Formato de fecha del grid | Fecha formateada | `Expediente.cshtml:243-259` `formatFechaHoraAmPm` + `render` columna `Fecha` (línea 269) → `dd/MM/yyyy hh:mm am/pm` | ✅ COMPLIANT |
| 5 | Formato de fecha del grid | Fecha nula | `formatFechaHoraAmPm` retorna `''` para null/''/inválida (líneas 246, 253) | ✅ COMPLIANT |
| 6 | Aislamiento del change | Vistas no afectadas | `Consulta/Index.cshtml:71-78` conserva Descripción (`name="Descripcion"`) y Receta (`name="DescripcionReceta"`); `Cita/Index.cshtml` sin marcadores del expediente; `Historial/Index.cshtml` intacto | ✅ COMPLIANT |

**Resumen de cumplimiento**: 14/14 escenarios implementados; 1 (Id inválido) con desviación spec↔design heredada (HTTP 200 vs "400").

---

## Correctness (Static — Structural Evidence)

| Requirement | Estado | Notas |
|-------------|--------|-------|
| SP solo-Citas (sin UNION) | ✅ Implementado | `sp_nh_Historial.sql:26-31` single SELECT `FROM dbo.Cita`; `'Cita' AS Origen`; `ORDER BY c.FechaCita DESC`. BD: `UNION_present=0`, `FROM_Consulta_present=0`, `FROM_Cita_present=1`. |
| Patrón idempotente DROP+CREATE | ✅ Implementado | `sp_nh_Historial.sql:12-16` `IF OBJECT_ID(...) IS NOT NULL DROP PROCEDURE ...` + `GO` + `CREATE PROCEDURE` plano (sin wrapper `EXEC('...')`). |
| `sp_nh_GetTratamientosPorPaciente` existe en BD | ✅ Implementado | BD: `SQL_STORED_PROCEDURE`, `modify_date=2026-10-16` (recreado hoy, fix aplicado). |
| Formato fecha `dd/MM/yyyy hh:mm am/pm` (sin moment.js) | ✅ Implementado | `formatFechaHoraAmPm` + `pad2`; defensivo `/Date(ms)` e ISO; sin referencia a moment.js. |
| Modal Nuevo Tratamiento: Comentarios `<textarea rows="4">` | ✅ Implementado | `Expediente.cshtml:161`. |
| Modal Nueva Consulta: sin Descripción/Receta, Comentarios `rows="4"` | ✅ Implementado | `Expediente.cshtml:183-229` (sin `name="Descripcion"`/`name="DescripcionReceta"`). |
| DTO/endpoint/DbWrapper intactos (sin cambios de contrato) | ✅ Implementado | `TratamientoHistoricoDTO` conserva `Id, Fecha, TratamientoNombre, Origen, Comentarios`; endpoint/DbWrapper/Service/HttpClient no tocados por este change. |
| Razor C# 5 (sin `?.` / `$""`) + BOM UTF-8 | ✅ Implementado | `Expediente.cshtml` sin `?.`/`$""`; BOM UTF-8 confirmado en `Expediente.cshtml` y `sp_nh_Historial.sql`. |

---

## Coherence (Design — D1–D5)

| Decisión | ¿Cumplida? | Notas |
|----------|-----------|-------|
| D1 SP devuelve solo Cita; `'Cita' AS Origen` + `ORDER BY FechaCita DESC` | ✅ Sí | Confirmado en archivo y en BD (sin UNION). |
| D2 Patrón DROP+CREATE idempotente | ✅ Sí | `DROP PROCEDURE ... GO CREATE PROCEDURE` plano. |
| D3 Reescribir `sp_nh_Historial.sql` in place (no `_v2`) | ✅ Sí | Un solo archivo `sp_nh_Historial.sql`; no existe `_v2`. |
| D4 Formato fecha JS manual `formatFechaHoraAmPm` (sin moment.js) | ✅ Sí | `Expediente.cshtml:243-259`, usado en `render` de columna Fecha. |
| D5 Comentarios→`<textarea rows="4">` en ambos modales; quitar Descripción/Receta del modal Consulta del expediente | ✅ Sí | `Expediente.cshtml:161` y `:226`; sin campos Descripción/Receta en `#modalConsultaExpediente`. |

---

## Issues Found

### CRITICAL (deben corregirse antes del archive)
Ninguno.

### WARNING (deberían corregirse, no bloquean)

- **WARNING-1 — Escenario "Id inválido → 400" no se cumple literalmente (tensión spec↔design heredada)**.
  - El spec (`spec.md` líneas 43-47) especifica `THEN responde 400 por validación` para `pacienteId no numérico o ≤ 0`. La implementación (no tocada por este change, heredada de `expediente-clinico-historial`) devuelve:
    - `pacienteId ≤ 0` (numérico): guard `<=0` → `IsSuccess=false` con **HTTP 200** ("El identificador del paciente es inválido.").
    - `pacienteId` no numérico: constraint `:long` no casa la ruta → **404**, no 400.
  - Es la misma tensión ya documentada como WARNING-1 en el verify-report de `expediente-clinico-historial` y resuelta deliberadamente por la convención del proyecto (`config.yaml`: "HTTP 200 siempre"). **Este change no modifica el endpoint**, por lo que no lo introduce, pero el texto del delta spec persiste con "400".
  - Recomendación: alinear el texto del spec en el archive (el escenario debe reflejar "IsSuccess=false con HTTP 200", no "400").

- **WARNING-2 — Tarea 5.1 marcada `[ ]` (BLOQUEADO) pero el build SÍ pasa (exit 0) vía VsDevCmd**.
  - La nota de la tarea atribuye el fallo a "falta `NuGet.targets` en `MSBuild\Current\Bin\amd64`". El MSBuild **directo** falla por ese motivo, pero el comando de build oficial (VsDevCmd + MSBuild) compila los 3 proyectos con exit 0. La marca `[ ]` es imprecisa: el change no tiene defecto de build.
  - Recomendación: marcar 5.1 como `[x]` (o aclarar que el build pasa con el entorno VsDevCmd) antes del archive.

### SUGGESTION (mejoras opcionales)

- **SUGGESTION-1 — Comentario obsoleto en `DbWrapper.Historial.cs`**.
  - La línea 14 dice `// Lista el historial de tratamientos de un paciente (Cita + Consulta).` pero el SP ya solo devuelve Cita. Actualizar a "(solo Cita)" para evitar confusión futura.
  - Archivo: `ConsultorioMedicoWebApi/DAL/DbWrapper.Historial.cs:14`.

- **SUGGESTION-2 — Verificación en navegador pendiente (tarea 5.5)**.
  - No ejecutada en entorno headless. Los cambios se verificaron estáticamente (grep) + aspnet_compiler + BD, pero el render real del grid (formato am/pm, textareas) y el flujo de guardado AJAX no se probaron en un browser. Se recomienda una pasada manual post-archive.
  - No bloquea: la lógica JS es determinista y el formato fue validado por inspección.

---

## Verdict

**PASS WITH WARNINGS**

La implementación está completa: SP solo-Citas confirmado en archivo y en BD (0/2 filas esperadas), formato de fecha y modales ajustados en `Expediente.cshtml` conforme a spec, aislamiento respetado (`Consulta/Index.cshtml` conserva Descripción/Receta; `Cita/Index.cshtml` y `Historial/Index.cshtml` intactos), build exit 0 y aspnet_compiler exit 0. Los 2 WARNING son no bloqueantes (tensión spec↔design "400 vs HTTP 200" heredada, y marca imprecisa de la tarea 5.1). El cambio está **listo para archive**.
