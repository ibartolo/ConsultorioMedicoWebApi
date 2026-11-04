# Verification Report — saas-registro-aislamiento (re-verificación tras Batch 5.1)

**Change**: `saas-registro-aislamiento`
**Mode**: Standard (strict_tdd = false, sin test runner)
**Branch**: `develop`
**Date**: 2026-11-04 (re-verificación Batch 5.1)

---

## Build & Verification Execution

| Check | Command | Result |
|-------|---------|--------|
| MSBuild Debug (solución) | `MSBuild.exe DESI.ConsultorioMedico.sln /t:Build /p:Configuration=Debug` | ✅ exit 0 (Entities/MVC/WebApi) |
| aspnet_compiler (vistas Razor) | `aspnet_compiler -p ConsultorioMedicoMVC -v / -f <tmp>` | ✅ exit 0 |
| Tests | — | ➖ Sin runner; verificación = MSBuild + aspnet_compiler |

MSBuild: `ConsultorioMedicoEntities -> …dll`, `ConsultorioMedicoMVC -> …dll`, `ConsultorioMedicoWebApi -> …dll`, `MSBUILD_EXIT=0`. aspnet_compiler v4.8.9037.0 `ASPNET_EXIT=0`. Sin errores ni warnings de compilación.

---

## Re-verificación de correcciones Batch 5.1

| Ítem (previo) | Estado | Evidencia |
|---------------|--------|-----------|
| W#1 trim inconsistente en registro | ✅ RESUELTO | `WebApi/Services/RegistroService.cs:29,60-75` `Normalizar()` recorta `NombreComercial`, `RazonSocial`, `Responsable`, `Calle`, `NoExterior`, `NoInterior`, `Colonia`, `Municipio`, `Estado`, `Telefono`, `CorreoContacto` ANTES de `RegistroValidator.Validar` y del insert; `DbWrapper.Registro` persiste los mismos valores DTO normalizados; RFC se normaliza (`Trim().ToUpperInvariant()`); contraseña NO se recorta. |
| S#1 política de contraseña server-side | ✅ RESUELTO | `Entities/Registro/RegistroValidator.cs:36-37` exige `Password.Length >= 6` y rechaza solo-espacios (`IsNullOrWhiteSpace`); conserva match de confirmación. |
| W#2 `GetAllTipoConsulta` sin filtro Estatus | ✅ RESUELTO | `sql/migraciones_saas_02_procedimientos.sql:895-908` `@SoloActivos BIT = 1` con `WHERE (@SoloActivos=0 OR Estatus=1) AND ConsultorioID=@ConsultorioID`. `DbWrapper.TipoConsulta.ObtenerTiposConsulta(usuario, empresaId, soloActivos)`. WebApi `List` (activos) / `ListAdmin` (todos). MVC dropdowns de Consulta/Historial usan `ConsultarTiposConsulta` (activos); `TipoConsulta/Index` (DataTable) usa `ConsultarTiposConsultaAdmin` (todos, con badge Estatus). |
| S#4 enlace login → registro | ✅ RESUELTO | `Views/Home/Autentication.cshtml:80` enlaza "Registrar consultorio" a `Registro/Index`; modal stub "Solicitar acceso" eliminado; login sigue neutro (sin branding). |
| S#5 id decorativo DatosInstitucion | ✅ RESUELTO | `Views/DatosInstitucion/Index.cshtml` ya no envía `?id=` a `Logo` ni `{ id }` a `Consultar` (`consultarInstitucionUrl = Url.Action("Consultar","DatosInstitucion")`; logo por `Url.Action("Logo",...)`). |
| S#7 SP legado `sp_nh_AutenticacionUsuario.sql` | ✅ RESUELTO | Cabecera DEPRECADO + solo `PRINT` (no ejecutable); redirige a `migraciones_saas_02_procedimientos.sql`. |
| W#3 `reset_datos_consultorio.sql` | ✅ DOCUMENTADO | Sin trackear, FUERA del cambio; no se borra/modifica; NO debe ejecutarse para SaaS (documentado en tasks.md §Batch 5.1 y apply-progress). |

---

## Correctness (Static — Isolation crítica re-verificada)

| Chequeo | Estado | Evidencia |
|---------|--------|-----------|
| Tenant solo desde claim/cookie (nunca cliente) | ✅ | WebApi `BaseController.EmpresaId`; MVC `TokenCookie.EmpresaID`; servicios fijan `ConsultorioID`/`CompaniaID` desde el claim. |
| Sin llamadas a SPs viejos sin `@ConsultorioID`/`@CompaniaID` | ✅ | `DbWrapper.*` inyecta parámetros de tenant en todas las lecturas/escrituras. |
| Sin caller de `sp_nh_AutenticacionUsuario` con `@Password` | ✅ | `DbWrapper.Autenticacion.cs:21-34` llama solo con `@UserName`; SP (02:984-1001) firma `@UserName NVARCHAR(50)` sin `@Password`. |
| Sin exposición de Password de usuario | ✅ | `DbWrapper.Usuario.cs:14-25` no lee `Password`; `AutenticarUsuario` retorna `usuario.Password = null`. |
| Sin `DatosFiscalUnico` global | ✅ | Solo comentarios; `DatosFiscales` resuelve tenant por `@ConsultorioID`. |
| Sin `int institucionId = 1` / `EmpresaId = 1` / `CompaniaID = 1` fallback | ✅ | grep en `.cs`/`.cshtml` sin coincidencias de fallback a 1; `_RecetaContenido.cshtml:6`, `DatosInstitucion/Index.cshtml:4` usan `int institucionId = 0` (sin fallback). |
| Ruta Anexo y logo con tenant | ✅ | `AnexoService.cs:99` `/Attachment/{empresaId}/{pacienteId}/...`; `DatosInstitucionService.cs:88` `/Attachment/{empresaId}/institucion/{guid}.ext`. |
| `Attachment/Web.config` presente | ✅ | `<deny users="*">` + `<system.webServer><security><authorization> deny` (visto). |
| Sin cambios a `-dermatologia` / `DB_9C7990_Consultorio` | ✅ | `git status`/`git diff` solo en proyectos develop + `sql/` + `openspec/`; sin cambios en `Web.config`/`DbWrapper.cs` (conexión). |

---

## Migraciones (estáticas, NO ejecutadas)

- **01_esquema** ✅ estático: 11 `COL_LENGTH ... ADD ConsultorioID`; backfill `=1` (11); FK `FK_<Tabla>_ConsultorioID` (11); `ALTER COLUMN ... NOT NULL` (11); `Usuario.Password NVARCHAR(250)`; `CompaniaID` backfill NULL→1 + NOT NULL + FK; corrección FKs `CitaPaquete` (`CitaID→Cita.Id`, `PacientePaqueteID→PacientePaquete.Id`); 6 unique indexes (`UX_Usuario_UserName`, `UX_DatosFiscales_RFC/RazonSocial/DatosInstitucionID`, `UX_DatosInstitucion_Nombre/Email`).
- **02_procedimientos** ✅ estático: `@SoloActivos` en `GetAllTipoConsulta`; parámetros de tenant en todos los SPs; `sp_nh_AutenticacionUsuario` redefinido (solo `@UserName`); contrato `Save*` (INSERT con `@Id=0`+`SCOPE_IDENTITY` / UPDATE `WHERE Id AND ConsultorioID`, `@@ROWCOUNT=0 → SELECT NULL`).
- **03_archivos** ✅ estático: `UPDATE LogoRuta → /Attachment/1/institucion/`; `UPDATE Anexo.Ruta → /Attachment/1/...` (idempotente). NO ejecutado.
- **Clon intacto**: no se ejecutó DDL/DML (solo `SET PARSEONLY` según apply-progress); no se movieron archivos.

---

## Coherence (Design)

| Decisión | Seguida | Nota |
|----------|---------|------|
| Flujo View→MVC→WebApi→DbWrapper→SP | ✅ | conservado |
| Tenant desde claim/cookie | ✅ | verificado |
| PBKDF2 (SHA256, 10000, salt16, hash32) | ✅ | `Helpers/Cryptography.cs`; `/token` → `AutenticarUsuario` (no texto plano) |
| Registro transaccional + rollback | ✅ | `DbWrapper.Registro` Begin/Commit/Rollback |
| Migraciones aditivas/idempotentes | ✅ | guards; solo `DROP` de FKs CitaPaquete incorrectas |
| `Paciente` conserva POCO (Factory Method EXCLUIDO) | ✅ | decisión de diseño |
| Branding por AJAX | ✅ | `_Layout.cshtml` → `DatosInstitucion/Branding` |

---

## Issues Found

### CRITICAL
Ninguno.

### WARNING
Ninguno (los 3 warnings previos quedaron resueltos/documentados).

### SUGGESTION (no bloqueantes)
1. **CP**: `RegistroValidator.cs:45` acepta `1..99999` (no exige exactamente 5 dígitos), mientras el cliente `cp5` exige 5. Alinear server-side.
2. **Teléfono**: cliente exige 10 dígitos; servidor permite `[0-9+\-()\s.]+`. Alinear.
3. **`CREATE OR ALTER`** (migración 02) vs redacción literal de design §2 / spec `paquetes-lectura` ("CREATE + IF OBJECT_ID"). Funcionalmente seguro e idempotente; solo desviación de redacción.
4. **SHA256/PBKDF2**: elección explícita (SHA256, no SHA1) — anotada, sin cambio.
5. **Movimiento físico de archivos** (8.3) pendiente: `Attachment/institucion/ → Attachment/1/institucion/` y carpetas huérfanas.
6. **`sql/reset_datos_consultorio.sql`**: untracked; asegurar que NO se incluya en el commit final (`git add` selectivo o `.gitignore`).
7. **(cosmético)** `RegistroValidator` evalúa `Password.Length < 6` antes de `IsNullOrWhiteSpace`; una contraseña corta de solo-espacios reporta "mínimo 6 caracteres" en vez de "solo espacios". Ambas rechazan; solo cambia el mensaje.

---

## Statement: migraciones NO ejecutadas / runtime pendiente

- Migraciones SQL `01/02/03` **NO ejecutadas** (solo `SET PARSEONLY`). Clon `db_9c7990_consultoriodesitst` intacto; `DB_9C7990_Consultorio`/`-dermatologia` intactos.
- Pendientes (no son fallas): **0.4** respaldo, **8.3** movimiento físico, **11.1–11.3** ejecución de migraciones (con aprobación), **matriz M1–M19**.
- No se ejecutó SQL ni se inspeccionó la BD en esta fase.

---

## Verdict

**PASS** (verificación estática/código — sin CRITICAL ni WARNING restantes)

La implementación es estructuralmente completa y coherente con proposal/specs/design/tasks. Build (MSBuild + aspnet_compiler) exit 0. Todas las correcciones de Batch 5.1 están aplicadas y verificadas en código; el aislamiento tenant crítico está confirmado estáticamente. Los únicos pendientes son pasos de ejecución humana (0.4, 8.3, 11.x) y la matriz de pruebas runtime M1–M19, que requieren las migraciones aplicadas en el clon.

## Next Recommended Step

1. Aprobar y ejecutar `01_esquema` → `02_procedimientos` → `03_archivos` en el clon (tras 0.4 respaldo y 8.3 movimiento físico), luego correr la matriz M1–M19.
2. No commitear `sql/reset_datos_consultorio.sql`.
3. Proceder a `sdd-archive` una vez que Phase 11 esté verde.
