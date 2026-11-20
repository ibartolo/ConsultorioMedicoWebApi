# Verification Report — licencias-pagos

**Change**: licencias-pagos
**Mode**: Standard (strict_tdd=false; sin xUnit — verificación = MSBuild + aspnet_compiler + revisión de código)
**Fecha**: 2026-11-19
**Status**: **pass** (con WARNINGS)

---

## Executive Summary

La implementación de **licencias-pagos** está completa y es consistente con `proposal.md`, `design.md` y los 5 delta specs. La migración SQL es aditiva e idempotente; las entidades, DAL, servicios y controladores (WebApi + MVC) reflejan las decisiones de diseño (licencia en `DatosInstitucion`, pago análogo a `EmpresaPago` sin `RequiereFactura`, tenant por claim, confirmación manual no expuesta, modo restringido vía flag en cookie). **MSBuild Debug → exit 0** y **aspnet_compiler → exit 0** fueron confirmados por ejecución real en esta sesión. Verify no ejecutó SQL (correcto); posteriormente el operador aplicó la migración 05 al clon SaaS. La tarea 6.4 (QA manual en clon SaaS) queda pendiente del operador, y la dependencia de orden de despliegue ya quedó resuelta al aplicar la migración antes del código.

---

## Completeness

| Metric | Value |
|--------|-------|
| Tasks total | 35 |
| Tasks complete | 34 |
| Tasks incomplete | 1 |

Incompleta: `6.4 Manual (clon SaaS)` — QA manual pendiente del operador (esperado: apply no ejecuta SQL; operator-run).

---

## Build & Tests Execution

**Build (MSBuild Debug)** — ✅ Passed
```
ConsultorioMedicoEntities -> ...ConsultorioMedicoEntities.dll
ConsultorioMedicoMVC     -> ...ConsultorioMedicoMVC.dll
ConsultorioMedicoWebApi  -> ...ConsultorioMedicoWebApi.dll
EXITCODE=0
```

**aspnet_compiler (Razor views)** — ✅ Passed
```
ASP.NET versión 4.8.9037.0 ... EXITCODE=0
```

**Tests**: ➖ No runner (sin xUnit; config.yaml `strict_tdd: false`). Verificación = build + aspnet_compiler + revisión de código (PLAYBOOK §9).

**Coverage**: ➖ Not available.

### Post-verificación: migración 05 aplicada

- Servidor/BD: `sql8010.site4now.net` / `db_9c7990_consultoriodesitst`.
- Corrección previa: `PLAN` es palabra reservada en SQL Server; se usó `[Plan]` en el script.
- Primera ejecución: columnas, `ConsultorioPago`, FK/IX, 2 pagos piloto y 8 SPs creados.
- Segunda ejecución: idempotente, sin duplicados (`TotalPagos=2`, `PagosPiloto=2`).
- Consultorios 1/2: `Estatus=1`, `Plan=Basico`, `EsPeriodoPrueba=0`, vigencia `2026-11-19` → `2027-11-19`.
- `sp_nh_AutenticacionUsuario` devuelve Email + `FechaVigenciaFin`, `EsPeriodoPrueba`, `Plan`, `LicenciaEstatus`.

---

## Spec Coverage

| Spec | Requirement | Estado | Evidencia |
|------|-------------|--------|-----------|
| licencia-consultorio | Modelo de licencia en `DatosInstitucion` (sin tabla aparte) | ✅ Implemented | `DatosInstitucion.cs:31-35`; `migraciones...05.sql:47-90` |
| licencia-consultorio | Trial 60 días (Plan=Basico, EsPeriodoPrueba=1, Estatus=1) | ✅ Implemented | `sp_nh_SetLicenciaInicial` (sql:297-314); `DbWrapper.Registro.cs:167-173` |
| licencia-consultorio | Backfill consultorios 1 y 2 (anual desde hoy) | ✅ Implemented | sql:96-103 (idempotente `FechaVigenciaFin IS NULL`) |
| licencia-consultorio | Pantalla Licencia accesible a todos | ✅ Implemented | MVC `LicenciaController.Index`; `_Layout.cshtml:118-123` (menú sin restricción de roles) |
| licencia-consultorio | Plan Básico $1,200 fijo, sin captura/factura | ✅ Implemented | `LicenciaService.cs:309-315` (monto de config); view sin campo de monto |
| licencia-consultorio | Datos bancarios/monto desde `Web.config` | ✅ Implemented | MVC `Web.config:14-19`; `LicenciaController.cs:23-26` |
| licencia-consultorio | Migración solo clon SaaS | ✅ Implemented | sql:22-28 (solo `db_9c7990_consultoriodesitst`, no dermatologia) |
| pagos-licencia | Tabla `ConsultorioPago` sin `RequiereFactura` | ✅ Implemented | sql:109-158; `ConsultorioPago.cs` (sin RequiereFactura) |
| pagos-licencia | Carga PDF/JPG/JPEG/PNG ≤3 MB → Pendiente | ✅ Implemented | `LicenciaService.cs:24-26,134-145,160-166`; `sp_nh_GuardarPagoLicencia` |
| pagos-licencia | Extensión provisional solo si venció (no reduce futura) | ✅ Implemented | sql:251-255 (`FechaVigenciaFin IS NULL OR < GETDATE()`) |
| pagos-licencia | Confirmación manual (+1 año, no expuesta) | ✅ Implemented | `sp_nh_ConfirmarPagoLicencia` (sql:263-293); sin ruta en `LicenciaController` |
| pagos-licencia | Correo best-effort (no revierte registro) | ✅ Implemented | `LicenciaService.cs:245-301` (try/catch + Log.Error) |
| pagos-licencia | Historial filtrado por tenant | ✅ Implemented | `sp_nh_GetPagosLicencia` + `DbWrapper.Licencia.cs:83-102` |
| pagos-licencia | Acceso a archivo solo por API/MVC con tenant | ✅ Implemented | `Attachment/Web.config` (`deny users="*"`); `sp_nh_GetPagoLicenciaById` |
| pagos-licencia | Backfill pago Confirmado $1,200 `system` 1/2 | ✅ Implemented | sql:164-186 (idempotente `NombreArchivo='Licencia anual piloto'`) |
| licencia-vencida-... | Login no bloqueado | ✅ Implemented | `HomeController.cs:108-123`; `sp_nh_AutenticacionUsuario` LEFT JOIN no filtra vigencia |
| licencia-vencida-... | Confinamiento global + allowlist (sin loop) | ✅ Implemented | `FilterConfig.cs:28-67` |
| licencia-vencida-... | Aviso visible en Licencia | ✅ Implemented | `Index.cshtml:11-24,184-186` |
| licencia-vencida-... | Menú limitado | ✅ Implemented | `_Layout.cshtml:45-117` (`@if (!vigenciaVencida)`) |
| licencia-vencida-... | Renovación → re-login | ✅ Implemented | flag en cookie; `LogOut` recalcula al re-ingresar |
| licencia-vencida-... | Retrocompatibilidad sesiones previas | ✅ Implemented | `TokenCookie.cs:20` (bool default false) |
| autenticacion-saas | `TokenCookie.VigenciaVencida` calculado en login | ✅ Implemented | `TokenCookie.cs:20`; `HomeController.cs:111-122` |
| autenticacion-saas | `sp_nh_AutenticacionUsuario` preserva contrato + Email + campos licencia | ✅ Implemented | sql:356-379; `DbWrapper.Autenticacion.cs:21-39` |
| registro-saas | `sp_nh_SetLicenciaInicial` en transacción + rollback | ✅ Implemented | `DbWrapper.Registro.cs:52-59,167-173` |

**Coverage summary**: 24/24 requirements con evidencia estructural. Sin escenarios UNTESTED a nivel de código (los escenarios de extensión no permitida / tamaño excedido están cubiertos por validación en `LicenciaService.cs:134-145` + cliente en `Index.cshtml:240-247`).

---

## Correctness (Static — Structural Evidence)

| Requirement | Status | Notes |
|------------|--------|-------|
| Columnas licencia en `DatosInstitucion` | ✅ | 5 columnas con defaults correctos; `Plan` default `Basico` |
| `ConsultorioPago` esquema | ✅ | Columnas exactas; FK + IX; sin `RequiereFactura` |
| SPs firmas | ✅ | 8 SPs (7 nuevos + redefinición auth) coinciden con design |
| `sp_nh_AutenticacionUsuario` contrato | ✅ | Solo `@UserName`; LEFT JOIN; `di.Estatus AS LicenciaEstatus`; +`Email` |
| Tenant por claim | ✅ | `BaseController.EmpresaId` (claim `empresaId`, `Startup.cs:63-64`) |
| Confirmación manual no expuesta | ✅ | Sin acción `Confirmar` en WebApi ni MVC |
| Sin captura de monto / factura | ✅ | Monto de config; view sin input de monto; sin CFDI |

---

## Coherence (Design)

| Decision | Followed? | Notes |
|----------|-----------|-------|
| Columnas en `DatosInstitucion` (no tabla aparte) | ✅ Yes | |
| LEFT JOIN en auth (1 roundtrip) | ✅ Yes | |
| Flag en cookie (no bloqueo de login) | ✅ Yes | |
| Confirmación manual BD/SP | ✅ Yes | |
| Monto fijo $1,200 desde config | ✅ Yes | |
| Email del uploader vía auth SP + Email | ✅ Yes | |
| `QUOTED_IDENTIFIER ON` en migración | ✅ Yes | sql:37-42 |
| Allowlist restringida sin `DatosInstitucion.Branding` | ✅ Yes | `FilterConfig.cs:28-33`; `_Layout.cshtml:227` omite branding en restringido |

---

## Issues Found

### CRITICAL
None.

### WARNING
1. **Dependencia de orden de despliegue (migración debe aplicarse ANTES del código)** — `DbWrapper.Autenticacion.cs:27-33` (`ObtenerUsuarioPorUserName`) y `DbWrapper.DatosInstitucion.cs:36-40` (`LlenarDatosInstitucion`) ahora leen columnas (`Email`, `FechaVigenciaFin`, `EsPeriodoPrueba`, `Plan`, `LicenciaEstatus`, `Estatus`) que solo devuelven los SP redefinidos/`SELECT *` **después** de ejecutar la migración 05. El SP anterior (`migraciones_saas_02_procedimientos.sql:986-1003`) devolvía solo `Id, UserName, Password, Estatus, CompaniaID` (sin `Email`). Hasta que el operador ejecute `migraciones_saas_05_licencias_pagos.sql`, el login (`/token` y `Home/LogIn`) y la pantalla Datos Institución lanzarán `IndexOutOfRangeException` (capturado → null → login falla). **Mitigación**: desplegar código y migración atómicamente; el operador ejecuta el SQL antes de publicar. Ref spec: `autenticacion-saas` / tarea 1.8.
2. **QA manual pendiente (tarea 6.4)** — No ejecutado (alta→trial 60d; backfill 1/2; upload→Pendiente+correo+extensión; confirmar→+1año; vencido→restringido+re-login). Esperado en esta fase (sin ejecución de SQL por el agente). Ref: tasks.md 6.4, proposal Success Criteria.

### SUGGESTION
1. `sp_nh_GuardarPagoLicencia` extiende `FechaVigenciaFin` también cuando es `NULL` (no solo vencida). Documentado en el SP y benigno, pero ligeramente más amplio que el literal "si la licencia ya venció". Sin acción requerida. Ref: sql:251-255.
2. MVC `LicenciaController.Descargar` re-fetchea el historial completo para resolver el nombre del archivo (roundtrip extra). Podría devolverse el nombre desde la WebApi. Ref: MVC `LicenciaController.cs:59-66`.
3. La cabecera "Monto anual" muestra `$1200 MXN` (valor crudo de config) en lugar de `$1,200.00`. Cosmético. Ref: `Index.cshtml:84`.

---

## Verification Evidence

- `MSBuild DESI.ConsultorioMedico.sln /t:Build /p:Configuration=Debug` → **exit 0** (ejecutado en esta sesión).
- `aspnet_compiler -p ConsultorioMedicoMVC -v /` → **exit 0** (ejecutado en esta sesión).
- BOM UTF-8 presente en `Template_PagoRecibido.html`, `migraciones_saas_05_licencias_pagos.sql`, `Views/Licencia/Index.cshtml` (verificado por lectura de bytes).
- Registros en `.csproj` confirmados (Entities/MVC/WebApi).
- `Web.config` MVC (PagoBanco/Clabe/Titular/Correos/MontoAnual) y WebApi (Correos/MontoAnual) presentes con los valores del design.
- `Attachment/Web.config` niega acceso directo (`deny users="*"`).

## Risks

- **Orden de despliegue**: si el código se publica sin ejecutar la migración 05, el login se rompe (ver WARNING 1). Mitigado si el operador ejecuta el SQL primero.
- **Renovación no libera sesión** (documentado): el flag vive en la cookie; requiere re-login. Mitigado por aviso en UI y `LogOut`.
- **Migración no ejecutada aún**: el clon SaaS no tiene las columnas/SPs hasta que el operador corra el script; backfill 1/2 pendiente de datos reales.

## Next Recommended

1. Operador ejecuta `sql/migraciones_saas_05_licencias_pagos.sql` en `db_9c7990_consultoriodesitst` (develop/main) — **antes** de desplegar el código.
2. Operador realiza QA manual 6.4 (alta, backfill 1/2, upload+correo+extensión, confirmación, vencido→restringido+re-login, aislamiento de tenant).
3. Tras QA manual, `sdd-archive` — recordando la dependencia: `saas-registro-aislamiento` debe archivar ANTES que `licencias-pagos`.

## Skill Resolution

`injected` — el orquestador inyectó el bloque `## Project standards` (netframework-mvc-webapi / sdd-verify) en el prompt de lanzamiento; no se cargaron SKILL.md adicionales.
