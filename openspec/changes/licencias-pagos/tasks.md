# Tasks: Licencias y Pagos (licencias-pagos)

## Fase 1 — Migración / BD
- [x] 1.1 Crear `sql/migraciones_saas_05_licencias_pagos.sql` abriendo con `SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET NOCOUNT ON;` (obligatorio por índices filtrados de `DatosInstitucion`).
- [x] 1.2 Columnas idempotentes en `DatosInstitucion` con `IF COL_LENGTH(...) IS NULL` + `DF_` default: `Estatus BIT 1`, `FechaVigenciaInicio/FechaVigenciaFin DATETIME NULL`, `EsPeriodoPrueba BIT 0`, `Plan NVARCHAR(20) 'Basico'`.
- [x] 1.3 Backfill consultorios 1/2: `FechaVigenciaInicio=GETDATE()`, `FechaVigenciaFin=DATEADD(YEAR,1,GETDATE())`, `EsPeriodoPrueba=0`, `Plan='Basico'`, `Estatus=1` `WHERE Id IN (1,2) AND FechaVigenciaFin IS NULL`.
- [x] 1.4 Tabla `ConsultorioPago` (`IF OBJECT_ID IS NULL`): columnas exactas del design + PK `Id IDENTITY`, FK→`DatosInstitucion(Id)` (guard `sys.foreign_keys`), `IX_ConsultorioPago_ConsultorioID` (guard `sys.indexes`).
- [x] 1.5 Backfill pagos Confirmados 1/2 idempotente por `NombreArchivo='Licencia anual piloto'`: `Monto=1200.00`, `EstatusPagoId=2`, `FechaCarga=FechaConfirmacion=GETDATE()`, `SubidoPor=ConfirmadoPor='system'`, `Notas='Licencia anual piloto'`, `ArchivoRuta=NULL`, `Estatus=1`.
- [x] 1.6 SPs `CREATE OR ALTER` (firmas del design): `sp_nh_GetLicencia`, `sp_nh_GetPagosLicencia`, `sp_nh_GuardarPagoLicencia`, `sp_nh_ConfirmarPagoLicencia`, `sp_nh_SetLicenciaInicial`, `sp_nh_GetPrimerUsuarioConsultorio`, `sp_nh_GetPagoLicenciaById`.
- [x] 1.7 Redefinir `sp_nh_AutenticacionUsuario`: `LEFT JOIN DatosInstitucion`, +`Email`, alias `di.Estatus AS LicenciaEstatus`.
- [x] 1.8 Revisar idempotencia (sin `DROP`). ⚠ **PASO MANUAL DEL OPERADOR**: apply solo crea el script; NO ejecuta SQL. El operador lo corre en `db_9c7990_consultoriodesitst`.

## Fase 2 — Entidades / auth
- [x] 2.1 `Catalogos/DatosInstitucion.cs`: +`Estatus`, `FechaVigenciaInicio/Fin`, `EsPeriodoPrueba`, `Plan`.
- [x] 2.2 Crear `Catalogos/ConsultorioPago.cs` (POCO plano).
- [x] 2.3 Crear `Catalogos/LicenciaDTO.cs`.
- [x] 2.4 `Autenticacion/Usuario.cs`: +`FechaVigenciaFin`, `EsPeriodoPrueba`, `Plan`, `LicenciaEstatus (bool?)` (`Email` ya existe).
- [x] 2.5 `Seguridad/TokenCookie.cs`: +`VigenciaVencida (bool)` (default `false` → retrocompatible).
- [x] 2.6 `DAL/DbWrapper.Autenticacion.cs`: mapear `Email` + columnas licencia en `ObtenerUsuarioPorUserName`.
- [x] 2.7 `Services/AutenticacionService.cs`: copiar campos licencia al `UsuarioDTO` (no rompe `/token`).
- [x] 2.8 `HomeController.LogIn`: `VigenciaVencida = FechaVigenciaFin.HasValue && < GetDateCenterMexico()` (NULL no restringe); setear en `tokenCookie`.

## Fase 3 — Registro
- [x] 3.1 `DAL/DbWrapper.Registro.cs`: helper `InicializarLicencia(nuevoId)` → `sp_nh_SetLicenciaInicial`; invocarlo tras `InsertarDatosInstitucion` y antes de `CommitTransaction()`. `@@ROWCOUNT<=0` → rollback total.

## Fase 4 — WebApi
- [x] 4.1 Crear `DAL/DbWrapper.Licencia.cs`: `ObtenerLicencia`, `ObtenerPagosLicencia`, `GuardarPagoLicencia`(→int), `ObtenerPagoLicenciaById`, `ObtenerPrimerUsuarioConsultorio`. Tenant siempre `empresaId`.
- [x] 4.2 Crear `Services/LicenciaService.cs`: validar ext `.pdf/.jpg/.jpeg/.png` ≤3MB, guardar `Attachment/{empresaId}/pagos/{guid}.ext`, monto `PagoMontoAnual`, insertar pago, correo best-effort (`EmailHelper` en try/catch + `Log.Error`), `ObtenerBytesPago`.
- [x] 4.3 Crear `Controllers/LicenciaController.cs` `[Authorize][RoutePrefix("api/Licencia")]`: `GET ""`, `GET Pagos`, `POST SubirComprobante`, `GET Descargar/{pagoId:int}`. `sp_nh_ConfirmarPagoLicencia` NO expuesto.
- [x] 4.4 Crear `Template/Template_PagoRecibido.html` (placeholder `{{UrlLicencia}}`, BOM UTF-8).
- [x] 4.5 `WebApi/Web.config`: +`PagoCorreoComprobante`, `PagoCorreoAviso`, `PagoMontoAnual`.

## Fase 5 — MVC
- [x] 5.1 Crear `DAL/HttpClientConnection.Licencia.cs`: `ObtenerLicencia`, `ObtenerPagos`, `SubirComprobante(MultipartFormDataContent)`, `DescargarComprobante(int)→byte[]`.
- [x] 5.2 Crear `Services/LicenciaService.cs` (passthrough + validación).
- [x] 5.3 Crear `Controllers/LicenciaController.cs`: `Index` (ViewBag `Pago*` + `VigenciaVencida`), `Consultar`, `ConsultarPagos`, `SubirComprobante`, `Descargar(int)→File`.
- [x] 5.4 Crear `Views/Licencia/Index.cshtml`: card licencia + datos bancarios/monto + subida (`.pdf,.jpg,.jpeg,.png`, máx 3MB) + DataTable historial + banner si vencido; Intermedio/Avanzado "En desarrollo".
- [x] 5.5 `Views/Shared/_Layout.cshtml`: `<li>Licencia</li>` (todos); en restringido ocultar resto del menú (solo Licencia + Cerrar Sesión) y NO ejecutar el AJAX de `DatosInstitucion.Branding` (branding por defecto).
- [x] 5.6 `App_Start/FilterConfig.cs`: allowlist restringida `Licencia.*`+`Home.LogOut` (sin `Branding`); redirigir a `Licencia`; `IsAjaxRequest()` → `JsonResult { IsSuccess=false, Message="Licencia vencida — modo restringido" }`. Sin loop.
- [x] 5.7 `MVC/Web.config`: +`PagoBanco`, `PagoClabe`, `PagoTitular`, `PagoCorreoComprobante`, `PagoCorreoAviso`, `PagoMontoAnual=1200`.
- [x] 5.8 Registrar en `.csproj` (Entities/WebApi/MVC): nuevos `.cs`, `.cshtml` (`<Content>`), `Template_PagoRecibido.html`.

## Fase 6 — Verificación
- [x] 6.1 `MSBuild DESI.ConsultorioMedico.sln /t:Build /p:Configuration=Debug` → exit 0.
- [x] 6.2 `aspnet_compiler -p ConsultorioMedicoMVC -v /` → exit 0.
- [x] 6.3 Revisar migración re-ejecutable (2 runs sin error) + `QUOTED_IDENTIFIER ON`.
- [ ] 6.4 Manual (clon SaaS): alta→trial 60d; 1/2→anual+Confirmado $1,200; upload→Pendiente+correo+extensión; confirmar→+1año; vencido→restringido+re-login libera; tenant aísla historial/descarga; sin captura de monto/factura; Intermedio/Avanzado "En desarrollo".
- [x] 6.5 Verificar sin acceso directo a `Attachment` (descarga solo por proxy API/MVC con tenant).

## Definition of Done
Build + aspnet_compiler exit 0; migración idempotente re-ejecutable; todos los criterios de `proposal.md` y escenarios de specs cubiertos; sin SQL ejecutado en esta fase y sin tocar `-dermatologia`/`DB_9C7990_Consultorio`.

## Out of scope
Brevo; funcionalidad real Intermedio/Avanzado; facturación/CFDI/`RequiereFactura`/captura de monto; pantalla admin de pagos; conciliación/pasarela/notificaciones de vencimiento; roles/permisos; deploy a producción; archivar cambios previos.

> **Orden de archive**: `saas-registro-aislamiento` (change activo) debe archivar ANTES que este cambio, por la dependencia de tenant-por-claim; no se añaden tareas de archive aquí.
