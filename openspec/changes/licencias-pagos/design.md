# Design: Licencias y Pagos (licencias-pagos)

## Technical Approach

Replicar HelpDesk sobre el patrón actual `Vista → MVC → WebApi → DbWrapper → SP` (ADR.NET + `sp_nh_*`), sin roles/permisos. La licencia vive en `DatosInstitucion` (sin tabla aparte); los pagos en una nueva `ConsultorioPago` (análoga a `EmpresaPago` sin `RequiereFactura`). El tenant se resuelve del claim OAuth `empresaId` en WebApi y de la cookie `TokenCookie.EmpresaID` en MVC, nunca del cliente. Al subir comprobante se inserta un pago **Pendiente** y, si venció, se extiende `FechaVigenciaFin` al fin del día; la confirmación es manual (BD/SP) y extiende +1 año. El modo restringido se materializa con `TokenCookie.VigenciaVencida` (calculado en login) y un guard que confina a `Licencia`/`Home.LogOut`/estáticos.

## Architecture Decisions

| Decisión | Opciones consideradas | Elección y rationale |
|---|---|---|
| Modelo de licencia | Tabla `Licencia` separada vs columnas en `DatosInstitucion` | **Columnas en `DatosInstitucion`**: 1:1 con el tenant, sin joins extra, igual que `FechaVigencia*` en HelpDesk `Empresa`. |
| Lectura de licencia en auth | SP nuevo de licencia (2º roundtrip) vs JOIN en `sp_nh_AutenticacionUsuario` | **LEFT JOIN en `sp_nh_AutenticacionUsuario`**: un solo roundtrip; devuelve `FechaVigenciaFin/EsPeriodoPrueba/Plan/Estatus(alias LicenciaEstatus)/Email` sin romper `/token`. |
| Cálculo de vencimiento | Bloquear login vs flag en cookie | **Flag en cookie** (`VigenciaVencida`): login no se bloquea; el guard global lo lee sin consultas por request (replica HelpDesk). |
| Confirmación de pago | Pantalla admin vs manual BD/SP | **Manual (SP `sp_nh_ConfirmarPagoLicencia`)**: sin pantalla de admin; el correo es el aviso. |
| Monto | Capturable vs fijo | **Fijo $1,200.00** desde `PagoMontoAnual` (WebApi). Sin factura/CFDI/`RequiereFactura`. |
| `Usuario` como origen de email del uploader | Param desde MVC vs `sp_nh_AutenticacionUsuario`+Email | **Extender `sp_nh_AutenticacionUsuario` para devolver `Email`**: la cookie no guarda email; se reutiliza el SP ya modificado. |

### Decisión: Naming collision `Estatus`

- `DatosInstitucion.Estatus` = licencia/consultorio activo (BIT, default 1). C# `DatosInstitucion.Estatus`.
- `ConsultorioPago.Estatus` = soft-delete (BIT, default 1). C# `ConsultorioPago.Estatus`.
- En `sp_nh_AutenticacionUsuario` (JOIN Usuario+DatosInstitucion) se **aliasa** `di.Estatus AS LicenciaEstatus` para no chocar con `Usuario.Estatus`; C# `Usuario.LicenciaEstatus` (`bool?`).
- `sp_nh_GetLicencia` devuelve `Estatus` (sin otra columna `Estatus` en su result set).

## Data Model

**`DatosInstitucion` (nuevas columnas, idempotente)**

| Columna | Tipo | Default | Notas |
|---|---|---|---|
| `Estatus` | BIT NOT NULL | 1 | licencia/consultorio activo |
| `FechaVigenciaInicio` | DATETIME NULL | — | |
| `FechaVigenciaFin` | DATETIME NULL | — | NULL = sin vencimiento (no restringe) |
| `EsPeriodoPrueba` | BIT NOT NULL | 0 | |
| `Plan` | NVARCHAR(20) NOT NULL | 'Basico' | `Basico\|Intermedio\|Avanzado` |

**`ConsultorioPago` (nueva)** — POCO, sin columnas de auditoría genéricas: `Id INT IDENTITY PK`, `ConsultorioID INT NOT NULL FK→DatosInstitucion(Id)`, `NombreArchivo NVARCHAR(255) NULL`, `ArchivoRuta NVARCHAR(500) NULL`, `Monto DECIMAL(10,2) NOT NULL`, `EstatusPagoId TINYINT NOT NULL DEFAULT 1` (1=Pendiente,2=Confirmado,3=Rechazado), `FechaCarga DATETIME NOT NULL`, `SubidoPor NVARCHAR(25) NOT NULL`, `FechaConfirmacion DATETIME NULL`, `ConfirmadoPor NVARCHAR(25) NULL`, `Notas NVARCHAR(500) NULL`, `Estatus BIT NOT NULL DEFAULT 1`. Índice `IX_ConsultorioPago_ConsultorioID`.

## Migration Order (`sql/migraciones_saas_05_licencias_pagos.sql`)

Abre con `SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;` (obligatorio: `DatosInstitucion` tiene índices filtrados; cualquier `UPDATE` en SP falla si el SP se crea con `QUOTED_IDENTIFIER OFF` — precedente `alter_sp_update_logo_institucion_quoted_identifier.sql`).

1. **Columnas** `DatosInstitucion` con `IF COL_LENGTH(...) IS NULL` + `ALTER TABLE ... ADD ... CONSTRAINT DF_... DEFAULT`.
2. **Backfill** Id 1 y 2 (idempotente): `UPDATE DatosInstitucion SET FechaVigenciaInicio=GETDATE(), FechaVigenciaFin=DATEADD(YEAR,1,GETDATE()), EsPeriodoPrueba=0, Plan='Basico', Estatus=1 WHERE Id IN (1,2) AND FechaVigenciaFin IS NULL;`
3. **Tabla** `ConsultorioPago` (`IF OBJECT_ID(...) IS NULL`) + `IX` + FK (guard `sys.foreign_keys`).
4. **Pagos confirmados** Id 1 y 2 (idempotente por `NombreArchivo='Licencia anual piloto'`): `Monto=1200.00, EstatusPagoId=2, FechaCarga=FechaConfirmacion=GETDATE(), SubidoPor=ConfirmadoPor='system', Notas='Licencia anual piloto', ArchivoRuta=NULL, Estatus=1`.
5. **SPs** `CREATE OR ALTER` (ver firma). Incluye redefinir `sp_nh_AutenticacionUsuario`.

Solo clon `db_9c7990_consultoriodesitst`; el script lo ejecuta manualmente el operador. Apply NO ejecuta SQL.

## Stored Procedures (signatures)

```sql
-- Licencia del tenant (una fila)
sp_nh_GetLicencia @ConsultorioID INT
  → SELECT Id AS ConsultorioID, Nombre, Estatus, FechaVigenciaInicio, FechaVigenciaFin, EsPeriodoPrueba, Plan
    FROM DatosInstitucion WHERE Id=@ConsultorioID;

-- Historial de pagos del tenant (soft-delete activo)
sp_nh_GetPagosLicencia @ConsultorioID INT
  → SELECT Id, ConsultorioID, NombreArchivo, ArchivoRuta, Monto, EstatusPagoId, FechaCarga, SubidoPor,
           FechaConfirmacion, ConfirmadoPor, Notas, Estatus
    FROM ConsultorioPago WHERE ConsultorioID=@ConsultorioID AND Estatus=1 ORDER BY FechaCarga DESC, Id DESC;

-- Alta comprobante: inserta Pendiente + extensión provisional al fin del día SI venció (no reduce vigencia futura)
sp_nh_GuardarPagoLicencia @ConsultorioID INT, @NombreArchivo NVARCHAR(255), @ArchivoRuta NVARCHAR(500),
                          @Monto DECIMAL(10,2), @Usuario NVARCHAR(25)
  → INSERT ... (EstatusPagoId=1, FechaCarga=GETDATE(), SubidoPor=@Usuario, Estatus=1); @PagoId=SCOPE_IDENTITY();
    UPDATE DatosInstitucion SET FechaVigenciaFin = DATEADD(SECOND,-1,DATEADD(DAY,1,CAST(GETDATE() AS DATE)))
      WHERE Id=@ConsultorioID AND (FechaVigenciaFin IS NULL OR FechaVigenciaFin < GETDATE());
    SELECT @PagoId;

-- Confirmación MANUAL (no expuesta en WebApi). Valida tenant del pago; +1 año desde max(hoy, vigencia actual)
sp_nh_ConfirmarPagoLicencia @PagoId INT, @ConsultorioID INT, @Usuario NVARCHAR(25), @Notas NVARCHAR(500)=NULL
  → valida EXISTS(ConsultorioPago WHERE Id=@PagoId AND ConsultorioID=@ConsultorioID AND Estatus=1) else -1;
    UPDATE ConsultorioPago SET EstatusPagoId=2, FechaConfirmacion=GETDATE(), ConfirmadoPor=@Usuario,
      Notas=ISNULL(@Notas,Notas) WHERE Id=@PagoId;
    UPDATE DatosInstitucion SET FechaVigenciaFin=DATEADD(YEAR,1,CASE WHEN FechaVigenciaFin>GETDATE() THEN FechaVigenciaFin ELSE GETDATE() END),
      EsPeriodoPrueba=0 WHERE Id=@ConsultorioID;
    SELECT 1;

-- Trial 60 días (solo en Registro). Sin @Usuario: DatosInstitucion no tiene auditoría.
sp_nh_SetLicenciaInicial @ConsultorioID INT
  → UPDATE DatosInstitucion SET Estatus=1, FechaVigenciaInicio=GETDATE(), FechaVigenciaFin=DATEADD(DAY,60,GETDATE()),
      EsPeriodoPrueba=1, Plan='Basico' WHERE Id=@ConsultorioID; SELECT @@ROWCOUNT;

-- Primer usuario (menor Id, activo) del tenant para el correo
sp_nh_GetPrimerUsuarioConsultorio @ConsultorioID INT
  → SELECT TOP 1 Id, UserName, Email FROM Usuario WHERE CompaniaID=@ConsultorioID AND Estatus=1 ORDER BY Id ASC;

-- Pago por Id validando tenant (descarga/autorización)
sp_nh_GetPagoLicenciaById @PagoId INT, @ConsultorioID INT
  → SELECT Id, ConsultorioID, NombreArchivo, ArchivoRuta, Monto, EstatusPagoId, FechaCarga, SubidoPor,
           FechaConfirmacion, ConfirmadoPor, Notas, Estatus
    FROM ConsultorioPago WHERE Id=@PagoId AND ConsultorioID=@ConsultorioID AND Estatus=1;

-- REDEFINIR (preserva contrato auth, agrega licencia + email)
sp_nh_AutenticacionUsuario @UserName NVARCHAR(50)
  → SELECT TOP 1 u.Id, u.UserName, u.Password, u.Email, u.Estatus, u.CompaniaID,
           di.FechaVigenciaFin, di.EsPeriodoPrueba, di.Plan, di.Estatus AS LicenciaEstatus
    FROM Usuario u LEFT JOIN DatosInstitucion di ON di.Id=u.CompaniaID
    WHERE u.UserName=@UserName ORDER BY u.Id ASC;
```

## Auth & Guard Flow

1. **WebApi auth** (`sp_nh_AutenticacionUsuario` + `DbWrapper.ObtenerUsuarioPorUserName`): mapear `Email`, `FechaVigenciaFin`, `EsPeriodoPrueba`, `Plan`, `LicenciaEstatus` en `Usuario`. `AutenticacionService.AutenticarUsuario` copia estos campos al `UsuarioDTO`. `/token` no cambia (solo usa `Id`/`CompaniaID`).
2. **MVC `HomeController.LogIn`** (PASO 1 ya devuelve `UsuarioDTO`): `bool vigenciaVencida = usuario.FechaVigenciaFin.HasValue && usuario.FechaVigenciaFin.Value < SessionHelper.GetDateCenterMexico();` → `tokenCookie.VigenciaVencida = vigenciaVencida;`. `FechaVigenciaFin == NULL` → **no** restringe. `EsPeriodoPrueba` y `LicenciaEstatus` **no** bloquean ni restringen (solo informativos). Login nunca se bloquea por licencia.
3. **`TokenCookie`**: `+ public bool VigenciaVencida { get; set; }`. Cookies previas sin el campo → `false` (retrocompatible).
4. **Guard `MVC/FilterConfig.AuthenticationFilter`**: tras el chequeo de sesión, si `tokenCookie.VigenciaVencida` y la clave `Controller.Action` NO está en `AccionesModoRestringido`, redirigir a `Licencia/Index`; para `IsAjaxRequest()` responder `JsonResult { IsSuccess=false, Message="Licencia vencida — modo restringido" }` (evita HTML roto). Sin loop (Licencia siempre permitida).
5. **Allowlist restringida**: `Licencia.*` (Index, Consultar, ConsultarPagos, SubirComprobante, Descargar) y `Home.LogOut`. Estáticos (Content/Scripts/img) no pasan por el filtro MVC. El `_Layout` NO llama `DatosInstitucion.Branding` en modo restringido; muestra el branding por defecto para respetar el allowlist estricto.
6. **Renovación → re-login**: el flag vive en la cookie; `Home.LogOut` cierra sesión y al re-ingresar se recalcula.

## Registro (transacción)

En `DbWrapper.Registro.RegistrarConsultorio`, tras `InsertarDatosInstitucion` (y antes de `CommitTransaction()`), invocar nuevo helper `InicializarLicencia(nuevoId)` → `sp_nh_SetLicenciaInicial @ConsultorioID=nuevoId`. Si `@@ROWCOUNT` no es > 0 → `RollbackTransaction()` + mensaje. Todo dentro de la misma transacción (rollback total si falla).

## WebApi

- `DAL/DbWrapper.Licencia.cs`: `ObtenerLicencia`, `ObtenerPagosLicencia`, `GuardarPagoLicencia` (devuelve `int` pagoId), `ObtenerPagoLicenciaById`, `ObtenerPrimerUsuarioConsultorio` (email/username). Todos reciben `empresaId` desde el claim (int). `sp_nh_ConfirmarPagoLicencia` **no** se expone (manual).
- `Services/LicenciaService.cs`: `ObtenerLicencia`, `ObtenerPagosLicencia`, `GuardarPagoLicencia(empresaId, usuario, HttpFileCollection)` (valida ext `.pdf/.jpg/.jpeg/.png` + ≤3 MB, guarda en `Attachment/{empresaId}/pagos/{guid}.ext`, monto=`PagoMontoAnual` config, inserta pago, correo best-effort), `ObtenerBytesPago`. `NotificarPagoRecibido`: destinatarios = primer usuario (`sp_nh_GetPrimerUsuarioConsultorio`) + uploader (`ObtenerUsuarioPorUserName(...).Email`) + fijos `PagoCorreoComprobante`/`PagoCorreoAviso`; plantilla `Template_PagoRecibido.html` con `{{UrlLicencia}}`; try/catch (SMTP no rompe el registro). `EmailHelper.EnviarEmail` **lanza** en fallo → envolver en try/catch y `Log.Error`.
- `Controllers/LicenciaController.cs` (`[Authorize][RoutePrefix("api/Licencia")]`): `GET ""` (licencia), `GET "Pagos"` (historial), `POST "SubirComprobante"` (multipart), `GET "Descargar/{pagoId:int}"` (proxy; resuelve el pago por tenant vía `sp_nh_GetPagoLicenciaById`). Tenant siempre de `EmpresaId`.

## MVC

- `Controllers/LicenciaController.cs`: `Index()` (ViewBag con `Pago*` + `VigenciaVencida`), `Consultar()`, `ConsultarPagos()`, `SubirComprobante(HttpPostedFileBase)` (multipart a WebApi), `Descargar(int id)` (proxy → `File(bytes, contentType, nombre)`). Acciones `[HttpPost]` devuelven JSON string.
- `Services/LicenciaService.cs` + `DAL/HttpClientConnection.Licencia.cs` (`ObtenerLicencia`, `ObtenerPagos`, `SubirComprobante(MultipartFormDataContent)`, `DescargarComprobante(int)→byte[]`).
- `Views/Licencia/Index.cshtml`: card plan/estatus/fechas + datos bancarios/monto (ViewBag) + subida (aceptar `.pdf,.jpg,.jpeg,.png`, máx 3 MB) + DataTable historial (Fecha, Archivo, Monto, Estatus, Comprobante). Banner rojo si `VigenciaVencida`. `Intermedio`/`Avanzado` = "En desarrollo".
- Menú: nuevo `<li>` "Licencia" en `_Layout.cshtml` (todos los usuarios). En restringido, ocultar el resto del menú y mostrar solo Licencia + Cerrar Sesión.
- `Web.config` MVC: `PagoBanco=STP`, `PagoClabe=646180401600408914`, `PagoTitular=Ivan Francisco Bartolo Castro`, `PagoCorreoComprobante=contacto@desipr.com`, `PagoCorreoAviso=bartolocastro@gmail.com`, `PagoMontoAnual=1200`. `Web.config` WebApi: `PagoCorreoComprobante`, `PagoCorreoAviso`, `PagoMontoAnual`.

## File (comprobante)

Guardar físico en WebApi `Attachment/{empresaId}/pagos/{guid}.ext` (igual patrón logo/anexo). `ArchivoRuta` = ruta parcial `/Attachment/...`. Descarga solo por proxy MVC→WebApi con `sp_nh_GetPagoLicenciaById` (tenant). Acceso directo ya bloqueado por `Attachment/Web.config` (`<deny users="*">`). Registros `system` (`ArchivoRuta=NULL`) → "Sin comprobante".

## File-by-file change map

| Archivo | Acción | Descripción |
|---|---|---|
| `sql/migraciones_saas_05_licencias_pagos.sql` | Crear | Columnas, backfill, tabla, pagos, SPs |
| `ConsultorioMedicoEntities/Catalogos/DatosInstitucion.cs` | Modificar | +`Estatus`,`FechaVigenciaInicio/Fin`,`EsPeriodoPrueba`,`Plan` |
| `ConsultorioMedicoEntities/Catalogos/ConsultorioPago.cs` | Crear | POCO pago |
| `ConsultorioMedicoEntities/Catalogos/LicenciaDTO.cs` | Crear | DTO de licencia |
| `ConsultorioMedicoEntities/Autenticacion/Usuario.cs` | Modificar | +`FechaVigenciaFin`,`EsPeriodoPrueba`,`Plan`,`LicenciaEstatus` (`Email` ya existe) |
| `ConsultorioMedicoEntities/Seguridad/TokenCookie.cs` | Modificar | +`VigenciaVencida` |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.Autenticacion.cs` | Modificar | Mapear nuevas columnas + `Email` en `ObtenerUsuarioPorUserName` |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.DatosInstitucion.cs` | Modificar | Mapear campos licencia en `LlenarDatosInstitucion` |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.Registro.cs` | Modificar | `InicializarLicencia(nuevoId)` en transacción |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.Licencia.cs` | Crear | Métodos licencia/pagos |
| `ConsultorioMedicoWebApi/Services/AutenticacionService.cs` | Modificar | Copiar campos licencia al DTO |
| `ConsultorioMedicoWebApi/Services/LicenciaService.cs` | Crear | Validación archivo + guardado + correo |
| `ConsultorioMedicoWebApi/Controllers/LicenciaController.cs` | Crear | Rutas GET/POST/descarga |
| `ConsultorioMedicoWebApi/Template/Template_PagoRecibido.html` | Crear | Plantilla correo |
| `ConsultorioMedicoWebApi/Web.config` | Modificar | +`PagoCorreoComprobante/Aviso/MontoAnual` |
| `ConsultorioMedicoMVC/Controllers/HomeController.cs` | Modificar | Calcular y setear `VigenciaVencida` |
| `ConsultorioMedicoMVC/Controllers/LicenciaController.cs` | Crear | Vistas + data access |
| `ConsultorioMedicoMVC/Services/LicenciaService.cs` | Crear | Passthrough + validación |
| `ConsultorioMedicoMVC/DAL/HttpClientConnection.Licencia.cs` | Crear | HTTP a WebApi |
| `ConsultorioMedicoMVC/Views/Licencia/Index.cshtml` | Crear | Pantalla Licencia |
| `ConsultorioMedicoMVC/App_Start/FilterConfig.cs` | Modificar | Allowlist restringida + AJAX |
| `ConsultorioMedicoMVC/Views/Shared/_Layout.cshtml` | Modificar | Menú Licencia + restringido |
| `ConsultorioMedicoMVC/Web.config` | Modificar | +`Pago*` |
| `*.csproj` (Entities/WebApi/MVC) | Modificar | Registrar nuevos `.cs`/`.cshtml`/template |

## Verification Plan

1. `MSBuild DESI.ConsultorioMedico.sln /t:Build /p:Configuration=Debug` → exit 0.
2. `aspnet_compiler -p ConsultorioMedicoMVC -v /` → exit 0.
3. Revisión de migración: re-ejecutable (sin error al correr dos veces), guards idempotentes, `SET QUOTED_IDENTIFIER ON`.
4. Manual (clon SaaS): alta → trial 60 días; consultorios 1/2 con anual + pago Confirmado $1,200; pantalla Licencia; subir comprobante → Pendiente + correo + extensión; confirmar → +1 año; vencido → restringido + re-login libera.
5. Tenant: el historial/descarga solo devuelve pagos del consultorio del claim.

## Risks / Rollback

| Riesgo | Mitigación |
|---|---|
| Loop redirect restringido | Allowlist `Licencia.*` + `Home.LogOut`; `_Layout` omite branding AJAX en restringido; AJAX responde JSON |
| Flag no se refresca sin re-login | Documentado: salir del modo restringido requiere re-login |
| Cookies previas sin flag | `bool` default `false` → no restringe |
| `QUOTED_IDENTIFIER OFF` al tocar `DatosInstitucion` (índices filtrados) | Migración abre con `SET QUOTED_IDENTIFIER ON`; todos los SPs nuevos/alterados en ese batch |
| Backfill 1/2 | Idempotente (`FechaVigenciaFin IS NULL` / `NombreArchivo='Licencia anual piloto'`) |
| Acceso directo al comprobante | `Attachment/Web.config` niega; descarga solo vía API con tenant |
| Rollback | Aditivo: `git checkout` revierte; restaurar respaldo del clon para datos |

## Open Questions

Ninguna bloqueante.
