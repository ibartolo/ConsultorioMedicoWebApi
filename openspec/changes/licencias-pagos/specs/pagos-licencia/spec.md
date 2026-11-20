# pagos-licencia Specification

## Purpose

Comprobante de pago por transferencia + confirmación manual + historial + correo. Análogo a HelpDesk (`EmpresaPago`) sin `RequiereFactura` ni captura de monto.

## Requirements

### Requirement: Tabla ConsultorioPago

El sistema MUST crear `ConsultorioPago` (análoga a `EmpresaPago`, sin `RequiereFactura`): `Id`, `ConsultorioID` FK a `DatosInstitucion`, `NombreArchivo`, `ArchivoRuta` (ambos NULL para registros `system`), `Monto DECIMAL(10,2)`, `EstatusPagoId` (1 Pendiente, 2 Confirmado, 3 Rechazado), `FechaCarga`, `SubidoPor`, `FechaConfirmacion`, `ConfirmadoPor`, `Notas`, `Estatus`.

#### Scenario: Esquema

- GIVEN la migración aplicada
- WHEN se inspecciona la BD
- THEN existe `ConsultorioPago` con FK a `DatosInstitucion` y sin `RequiereFactura`

### Requirement: Carga de comprobante

El sistema SHALL permitir subir un comprobante (transferencia). El archivo MUST ser PDF/JPG/JPEG/PNG y MUST pesar ≤3 MB. Se SHALL guardar en `Attachment/{consultorioId}/pagos/{guid}.ext` y se SHALL registrar un `ConsultorioPago` en estatus Pendiente.

#### Scenario: Carga exitosa

- GIVEN un usuario autenticado con un comprobante PDF ≤3MB
- WHEN sube el comprobante desde Licencia
- THEN se guarda el archivo y se crea un pago Pendiente

#### Scenario: Extensión no permitida

- GIVEN un archivo con extensión distinta (p.ej. `.docx`)
- WHEN intenta subirlo
- THEN se rechaza con mensaje claro

#### Scenario: Tamaño excedido

- GIVEN un archivo >3MB
- WHEN intenta subirlo
- THEN se rechaza con mensaje claro

### Requirement: Extensión provisional de vigencia al subir si venció

Al subir el comprobante, si la licencia ya venció, el sistema SHALL extender `FechaVigenciaFin` hasta el fin del día actual. MUST NOT reducir una vigencia futura.

#### Scenario: Extensión provisional

- GIVEN un consultorio con licencia vencida
- WHEN un usuario sube un comprobante
- THEN se extiende `FechaVigenciaFin` al fin del día actual

#### Scenario: Vigencia futura no se reduce

- GIVEN un consultorio con vigencia futura
- WHEN sube un comprobante
- THEN `FechaVigenciaFin` no cambia

### Requirement: Confirmación manual

La confirmación del pago SHALL ser manual (directo en BD/SP, sin pantalla de administración). Al confirmar, el sistema MUST marcar `EstatusPagoId=2` (Confirmado), `FechaConfirmacion` y `ConfirmadoPor`, y MUST extender `FechaVigenciaFin` +1 año desde la mayor entre hoy y la vigencia actual.

#### Scenario: Confirmación

- GIVEN un pago Pendiente
- WHEN se confirma manualmente
- THEN pasa a Confirmado y `FechaVigenciaFin` se extiende 1 año

### Requirement: Correo al subir comprobante

Al subir, el sistema SHALL enviar correo (best-effort) a: primer usuario del consultorio, quien sube, y los correos fijos DESI (`PagoCorreoComprobante`, `PagoCorreoAviso`), vía `EmailHelper`/SMTP con plantilla nueva. Si SMTP falla, el comprobante MUST quedar registrado.

#### Scenario: Envío exitoso

- GIVEN un comprobante subido
- WHEN se registra el pago
- THEN se notifica a los destinatarios sin romper el registro

#### Scenario: SMTP no disponible

- GIVEN SMTP no disponible
- WHEN se registra un comprobante
- THEN el pago queda registrado y el fallo se registra en log

### Requirement: Historial de pagos

El sistema SHALL listar el historial de pagos del consultorio del usuario (estado, fecha, archivo, monto), filtrado por tenant.

#### Scenario: Historial aislado

- GIVEN un usuario del consultorio 1
- WHEN abre el historial
- THEN ve solo los pagos del consultorio 1

### Requirement: Acceso a archivo solo por API/MVC con tenant

El acceso directo a `Attachment/` MUST estar bloqueado. Listar/descargar/subir MUST pasar por API/MVC con validación de tenant.

#### Scenario: Acceso directo denegado

- GIVEN una URL directa a un comprobante
- WHEN un usuario intenta acceder sin pasar por API
- THEN se deniega; solo vía API con validación de tenant

### Requirement: Pago Confirmado de consultorios 1 y 2 (backfill)

La migración SHALL crear un `ConsultorioPago` Confirmado por $1,200.00 para cada consultorio 1 y 2: `SubidoPor='system'`, `ConfirmadoPor='system'`, `FechaCarga/FechaConfirmacion=hoy`, `Notas='Licencia anual piloto'`, `NombreArchivo='Licencia anual piloto'`, `ArchivoRuta=NULL`.

#### Scenario: Backfill de pago

- GIVEN la migración ejecutada
- WHEN se revisan los pagos
- THEN los consultorios 1 y 2 tienen un pago Confirmado $1,200.00 con datos `system`
