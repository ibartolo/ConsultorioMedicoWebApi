# Verification Report

**Change**: calendario-citas-estados
**Version**: N/A (delta, capability nueva `cita`)
**Mode**: Standard (strict_tdd: false, sin test runner — verificación = MSBuild + aspnet_compiler, ver openspec/config.yaml)

---

## Completeness

| Metric | Value |
|--------|-------|
| Tasks total | 18 |
| Tasks complete | 17 |
| Tasks incomplete | 1 |

Incompletas:
- `6.4` E2E manual (render calendarios, tooltip, botón Eliminar, refetch, grid historial) — no ejecutado (sin harness de browser; verificado por evidencia estática + build + compilación Razor).

---

## Build & Tests Execution

**Build**: ✅ Passed (MSBuild 17.14 Debug, exit 0)

```
ConsultorioMedicoEntities -> ...\ConsultorioMedicoEntities\bin\Debug\...dll
ConsultorioMedicoMVC       -> ...\ConsultorioMedicoMVC\bin\ConsultorioMedicoMVC.dll
ConsultorioMedicoWebApi    -> ...\ConsultorioMedicoWebApi\bin\ConsultorioMedicoWebApi.dll
```

**aspnet_compiler (Razor views)**: ✅ Passed (exit 0, ASP.NET 4.8.9037.0)

**Tests**: ➖ No aplica (sin runner de tests en la solución: sin xUnit; ver config.yaml `testing`).

**Coverage**: ➖ Not available (config.yaml `coverage.available: false`).

**BD (solo lectura, cCon)**:
```
SELECT name FROM sys.procedures WHERE name IN ('sp_nh_DeleteCita','sp_nh_GetTratamientosPorPaciente')
```
→ ✅ retorna `sp_nh_DeleteCita` y `sp_nh_GetTratamientosPorPaciente` (ambos existen en `DB_9C7990_Consultorio`).

---

## Spec Compliance Matrix (evidencia estática + build; sin runner de tests)

| Requirement | Scenario | Evidencia | Result |
|-------------|----------|-----------|--------|
| Colores de eventos según estado | Evento pendiente (Estado=1 → #FD7E14) | `Views/Cita/Index.cshtml` `colorEstado()` L146-151 | ✅ COMPLIANT |
| Colores de eventos según estado | Evento sin estado (null/otro → #6C757D) | `colorEstado()` default return L150 | ✅ COMPLIANT |
| Tooltip con estado | Tooltip confirmado (Estado=2 → "Confirmada") | `nombreEstado()` L153-158 + `eventDidMount` L133-139 | ✅ COMPLIANT |
| Tooltip con estado | Tooltip sin estado (null → "Sin estado") | `nombreEstado()` default `'Sin estado'` L157 | ✅ COMPLIANT |
| Baja lógica vía SP | Baja lógica exitosa (Estatus=0, persiste) | `sql/sp_nh_Cita.sql` L112-131; SP existe en BD | ✅ COMPLIANT |
| Endpoint de eliminación | Eliminación exitosa (200, IsSuccess=true, Response=true) | `DbWrapper.Cita.cs` L132-159 | ✅ COMPLIANT |
| Endpoint de eliminación | Error de eliminación (IsSuccess=false, msg español) | `DbWrapper.Cita.cs` L141-146 + `CitaService.cs` guard L72-73 | ✅ COMPLIANT |
| Acción MVC y botón Eliminar | Botón visible al editar (Id>0) | `abrirModalCita()` L170-174 | ✅ COMPLIANT |
| Acción MVC y botón Eliminar | Botón oculto al crear (Id=0) | `abrirModalCita()` else `hide()` L172-173 | ✅ COMPLIANT |
| Acción MVC y botón Eliminar | Eliminación confirmada (POST + Swal + hide + refetch) | `eliminarCita()` L209-241 | ✅ COMPLIANT |
| Acción MVC y botón Eliminar | Eliminación cancelada (sin POST) | `if (!result.isConfirmed) return;` L219 | ✅ COMPLIANT |

**Compliance summary**: 11/11 escenarios cubiertos por evidencia estática; build + aspnet_compiler + SP en BD verificados en ejecución.

---

## Correctness (Static — Structural Evidence)

| Item | Status | Notes |
|------|--------|-------|
| Fix `Consulta/Index.cshtml` L119 `initialView: 'timeGridWeek',` | ✅ Implementado | coma presente en L119 |
| `colorEstado` (1 #FD7E14 / 2 #198754 / 3 #DC3545 / null #6C757D, texto #FFFFFF) | ✅ Implementado | L146-151 |
| `nombreEstado` (Pendiente/Confirmada/Cancelada/Sin estado) | ✅ Implementado | L153-158 |
| Colores aplicados en `events.push` | ✅ Implementado | L115, L120-122 |
| Tooltip `eventDidMount` | ✅ Implementado | L133-139 |
| Botón `#btnEliminarCita` con binding de click | ✅ Implementado | `onclick="eliminarCita()"` L73 (CRÍTICO confirmado) |
| Botón visible solo `Id>0` | ✅ Implementado | L170-174 |
| `eliminarCita()` SweetAlert + POST + `refetchEvents()` | ✅ Implementado | L209-241 |
| SP `sp_nh_DeleteCita` (CREATE idempotente, UPDATE Estatus=0) | ✅ Implementado | `sql/sp_nh_Cita.sql` L112-131 + cabecera L8 |
| WebApi `DbWrapper.Cita.cs` `EliminarCita` | ✅ Implementado | L132-159 (`ModelResponse<bool>`, `ExecuteNonQuery`) |
| WebApi `CitaService.cs` guard `id<=0` | ✅ Implementado | L72-73 |
| WebApi `CitaController.cs` `[HttpPost, Route("Eliminar/{id:int}")]` | ✅ Implementado | L32-34 |
| MVC `HttpClientConnection.Cita.cs` `EliminarCita(long id)` | ✅ Implementado | L20-21 |
| MVC `Services/CitaService.cs` `EliminarCita(long id)` | ✅ Implementado | L44-45 |
| MVC `Controllers/CitaController.cs` `[HttpPost] EliminarCita(long id)` | ✅ Implementado | L74-80 |

---

## Coherence (Design)

| Decision | Followed? | Notes |
|----------|-----------|-------|
| D1 Fix sintaxis Consulta | ✅ Yes | L119 |
| D2 Colores en construcción del evento | ✅ Yes | `backgroundColor/borderColor/textColor` en `events.push` |
| D3 Tooltip `eventDidMount` con `nombreEstado` default `'Sin estado'` | ✅ Yes | reconcile con spec |
| D4 `ModelResponse<bool>` (no `int`) | ✅ Yes | reconcile: spec es fuente de verdad |
| D5 `id` por ruta `Eliminar/{id:int}` | ✅ Yes | reconcile documentado |
| D6 SP sin `SELECT @@ROWCOUNT` (usa rowcount de `ExecuteNonQuery`) | ✅ Yes | `filas<=0` → "Cita no encontrada." |
| D7 SP anexado a `sp_nh_Cita.sql` + cabecera | ✅ Yes | 4º bloque + cabecera L8 |
| D8 `$.ajax data:{id}` (no serialize) | ✅ Yes | L223 |
| D9 `int` WebApi vs `long` MVC | ✅ Yes | coinciden |

---

## Issues Found

**CRITICAL** (must fix before archive):
- None.

**WARNING** (should fix):
- `6.4` E2E manual no ejecutado (sin harness de browser). Backend/BD/build/compilación verificados; falta validación manual en navegador.
- `sql/sp_nh_Historial.sql` usa patrón **DROP+CREATE** (L12-16), no "CREATE idempotente" como describe proposal/spec Nota 2. Archivo pre-existente y fuera del capability `cita` (solo se ejecuta); el SP `sp_nh_GetTratamientosPorPaciente` SÍ existe en BD. No bloquea, pero la descripción "solo CREATE con IF OBJECT_ID, sin DROP/ALTER" no aplica a este archivo.
- Endpoint real es `POST api/Cita/Eliminar/{id}` (route param) vs literal `POST api/Cita/Eliminar` de la spec; reconcilado en design D5 (el requisito "recibe el id" se preserva).

**SUGGESTION** (nice to have):
- Alinear la redacción de `sp_nh_Historial.sql` a CREATE idempotente (IF OBJECT_ID) para coherencia con la convención documentada, o aclarar en proposal/spec que ese archivo es DROP+CREATE histórico.

---

## Verdict

**PASS WITH WARNINGS**

Implementación del capability `cita` completa y correcta: build exit 0, aspnet_compiler exit 0, SPs presentes en BD productiva, y todos los puntos críticos (binding `onclick` del botón, colores, tooltip, guard `id<=0`, ruta de eliminación) verificados en código. Pendiente únicamente el E2E manual (6.4) y dos desviaciones menores documentadas (DROP+CREATE del historial y route param del endpoint), que no bloquean el archive.
