# Propuesta: Licencias y Pagos (licencias-pagos)

## Intent

Hoy no existe control de licencia ni cobro. Cada consultorio debe tener licencia anual (plan Básico $1,200.00 MXN), pagar por transferencia subiendo comprobante con confirmación manual, y quedar en modo restringido al vencer — replicando HelpDesk.

## Scope

### In Scope
- Licencia simple en `DatosInstitucion`: `Estatus BIT`, `FechaVigenciaInicio/FechaVigenciaFin DATETIME NULL`, `EsPeriodoPrueba BIT`, `Plan NVARCHAR(20)` (`Basico|Intermedio|Avanzado`, default `Basico`).
- Alta de consultorio: trial 60 días (`Plan=Basico`, `EsPeriodoPrueba=1`, `Estatus=1`).
- Backfill consultorios 1 y 2: licencia anual desde hoy + pago Confirmado $1,200.00 (`SubidoPor/ConfirmadoPor='system'`, `Notas='Licencia anual piloto'`, comprobante nulo).
- Pantalla `Licencia` (MVC) para todos los usuarios: plan, estatus, fechas, datos bancarios, monto, carga de comprobante e historial.
- Pago: transferencia + comprobante (PDF/imagen ≤3 MB) en `Attachment/{consultorioId}/pagos/{guid}.ext`; confirmación manual (BD/SP); historial visible.
- Tabla `ConsultorioPago` (análoga a `EmpresaPago`, sin `RequiereFactura`): `Id`, `ConsultorioID` FK, `NombreArchivo`, `ArchivoRuta` (nullable para system), `Monto DECIMAL(10,2)`, `EstatusPagoId` (1 Pendiente, 2 Confirmado, 3 Rechazado), `FechaCarga`, `SubidoPor`, `FechaConfirmacion`, `ConfirmadoPor`, `Notas`, `Estatus`.
- SPs: `sp_nh_GetLicencia`, `sp_nh_GetPagosLicencia`, `sp_nh_GuardarPagoLicencia`, `sp_nh_ConfirmarPagoLicencia`, `sp_nh_SetLicenciaInicial`, `sp_nh_GetPrimerUsuarioConsultorio`.
- Modo restringido (como HelpDesk): login NO se bloquea; `TokenCookie.VigenciaVencida`; guard redirige todo (salvo `Licencia`/`Home/LogOut`/estáticos) a `Licencia`; menú limitado; aviso con fecha y qué hacer.
- Correo al subir comprobante (primer usuario, uploader, correos fijos DESI) vía `EmailHelper`; plantilla nueva. Best-effort.
- Migración SQL aditiva (solo clon SaaS `db_9c7990_consultoriodesitst`).

### Out of Scope
- Brevo (solo planes Intermedio/Avanzado futuros); planes Intermedio/Avanzado solo modelados, sin precio ni funcionalidad.
- Facturación/CFDI; `RequiereFactura`; captura de monto.
- Pantalla de administración de pagos (confirmación directa en BD/SP).
- Conciliación automática / pasarela de pago / notificaciones de vencimiento.

## Capabilities

### New Capabilities
- `licencia-consultorio`: modelo de licencia en `DatosInstitucion` + pantalla Licencia + datos bancarios.
- `pagos-licencia`: comprobante de pago + historial + confirmación manual + correo.
- `licencia-vencida-modo-restringido`: confinamiento al vencer (flag cookie + guard + menú + aviso + extensión provisional).

### Modified Capabilities
- `autenticacion-saas` (change activo `saas-registro-aislamiento`): `TokenCookie.VigenciaVencida`; login NO bloquea por vigencia vencida.
- `registro-saas`: al alta, `sp_nh_SetLicenciaInicial` (trial 60 días).

## Approach

Replicar HelpDesk sobre el patrón actual (Vista→MVC→WebApi→DbWrapper→SP). La licencia se lee desde `DatosInstitucion` por tenant. Al subir comprobante se inserta `ConsultorioPago` Pendiente y, si venció, se extiende `FechaVigenciaFin` al fin del día actual; al confirmar (manual) +1 año. `AuthenticationFilter` redirige si `VigenciaVencida`; salir del modo restringido requiere re-login.

## Affected Areas

| Área | Impacto |
|---|---|
| `sql/migraciones_saas_05_licencias_pagos.sql` | New: columnas licencia, tabla pagos, SPs, backfill |
| `ConsultorioMedicoEntities/Catalogos/DatosInstitucion.cs` | Modified: +licencia |
| `ConsultorioMedicoEntities/Catalogos/ConsultorioPago.cs` | New |
| `ConsultorioMedicoEntities/Seguridad/TokenCookie.cs` | Modified: +`VigenciaVencida` |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.Licencia.cs`, `Services/LicenciaService.cs`, `Controllers/LicenciaController.cs` | New |
| `ConsultorioMedicoWebApi/Services/RegistroService.cs` | Modified: set trial |
| `ConsultorioMedicoMVC/Controllers/LicenciaController.cs`, `Services/`, `Views/Licencia/Index.cshtml` | New |
| `ConsultorioMedicoMVC/Controllers/HomeController.cs`, `App_Start/FilterConfig.cs`, `Views/Shared/_Layout.cshtml` | Modified: flag + guard + menú |
| `Web.config` (MVC) | Modified: `Pago*` appSettings |

## Risks

| Riesgo | Prob. | Mitigación |
|---|---|---|
| Loop de redirect en modo restringido | Baja | allowlist `Licencia`/`Home/LogOut`/estáticos |
| Renovación no libera sesión (flag en cookie) | Alta | documentar: requiere re-login |
| Cookies previas sin flag | Media | flag ausente se ignora |
| Backfill de 2 consultorios | Media | migración idempotente + respaldo del clon |
| Acceso directo a archivo de pago | Baja | sin acceso directo; list/upload/download por API/MVC |

## Dependencies

- HelpDesk como referencia (`EmpresaPago`, `PagoService`, modo restringido).
- `EmailHelper` + SMTP existentes.
- Change activo `saas-registro-aislamiento` (tenant por claim).

## Open Questions / Supuestos

Ninguna bloqueante. Supuestos: precio Básico $1,200.00 anual fijo; datos bancarios de HelpDesk (`PagoBanco=STP`, `PagoClabe=646180401600408914`, `PagoTitular=Ivan Francisco Bartolo Castro`, `PagoCorreoComprobante=contacto@desipr.com`, `PagoCorreoAviso=bartolocastro@gmail.com`); confirmación manual directo en BD/SP; comprobante nulo permitido en registros `system`.

## Success Criteria (Criterios de aceptación)

- [ ] Consultorio nuevo arranca trial 60 días (Plan=Basico).
- [ ] Consultorios 1 y 2 con licencia anual + pago Confirmado $1,200.00.
- [ ] Pantalla `Licencia` muestra plan/estatus/fechas/datos bancarios/historial y permite subir comprobante.
- [ ] Subir comprobante → pago Pendiente + correo + extensión al fin del día si venció.
- [ ] Confirmar manual → Confirmado +1 año.
- [ ] Vencido: login no bloquea, queda en `Licencia`, resto redirige; re-login tras renovar restablece acceso.
- [ ] MSBuild Debug + aspnet_compiler exit 0.

## Migration Impact

Nueva migración aditiva idempotente (guards `COL_LENGTH`/`OBJECT_ID`), solo en clon `db_9c7990_consultoriodesitst` (develop/main). NO tocar `-dermatologia`/`DB_9C7990_Consultorio`. No se ejecuta SQL en esta fase.

## Rollback Plan

Todo aditivo y versionable: `git checkout` revierte código y migración; columnas/tabla/SPs nuevos no alteran el esquema previo. Para datos, restaurar respaldo del clon previo a la migración. El flag `VigenciaVencida` ausente se ignora (retrocompatible).
