# Especificación: Registro SaaS

## Purpose

Registro público (ventana de captura sin autenticación) que crea un consultorio nuevo: tenant `DatosInstitucion` + fila fiscal `DatosFiscales` 1:1 + primer usuario admin (`UserName` = RFC normalizado, único global), todo en una sola transacción. No hay servicio de email, por lo que la contraseña se captura y se hashea antes de guardar. Sin roles/permisos, trial, vigencia, licencia ni pagos.

## Requirements

### Requirement: Ventana de registro público

El sistema MUST exponer una ventana pública de captura con los siguientes campos:

| Campo | Obligatorio | Mapeo |
|-------|-------------|-------|
| NombreComercial | Sí | `DatosInstitucion.Nombre` |
| RazonSocial | Sí | `DatosFiscales.RazonSocial` |
| RFC | Sí | `DatosFiscales.RFC` y `Usuario.UserName` (normalizado) |
| Responsable | Sí | `DatosInstitucion.MedicoNombre` |
| Direccion | Sí | `DatosInstitucion.Calle` (como mínimo) |
| CorreoContacto | Sí | `DatosInstitucion.Email` |
| Password + Confirmación | Sí | `Usuario.Password` (hash) |
| Ciudad | No | `DatosInstitucion.Municipio` |
| Estado | No | `DatosInstitucion.Estado` |
| CodigoPostal (int, D5) | No | `DatosInstitucion.CP` |
| Telefono | No | `DatosInstitucion.Telefono1` |
| NoExterior, NoInterior, Colonia | No | `DatosInstitucion.NoExterior/NoInterior/Colonia` |

`Pais` SHALL fijarse en "México" y MUST NOT capturarse. El sistema MUST NOT crear la columna `Direccion`; la dirección se reparte en las columnas existentes.

#### Scenario: Captura de un consultorio válido

- GIVEN un visitante con todos los campos obligatorios y contraseñas coincidentes
- WHEN envía el formulario de registro
- THEN el sistema valida, normaliza el RFC y crea el consultorio completo

#### Scenario: Contraseñas no coinciden

- GIVEN Password y Confirmación distintos
- WHEN el visitante envía el formulario
- THEN el sistema rechaza con mensaje claro y no crea nada

### Requirement: Normalización del RFC como UserName

El RFC SHALL ser el `UserName` del primer admin, normalizado con trim y mayúsculas. El sistema MUST validar el formato con la expresión de HelpDesk `[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}` (12–13 posiciones). `UserName` SHALL ser único global.

#### Scenario: RFC válido normalizado

- GIVEN el visitante teclea `"  xxe010101aa1 "`
- WHEN envía el registro
- THEN el sistema guarda `UserName` = `"XXE010101AA1"`

#### Scenario: RFC con formato inválido

- GIVEN un RFC que no cumple el patrón (ej. `"abc"` o `"1234567890"`)
- WHEN envía el registro
- THEN el sistema rechaza con mensaje claro de formato

### Requirement: Unicidad global sin fallback

RFC, correo de contacto, NombreComercial, RazonSocial y `UserName` MUST ser únicos a nivel global. `DatosFiscales.DatosInstitucionID` MUST ser 1:1 único. Si alguno ya existe, el sistema MUST rechazar con mensaje claro y MUST NOT aplicar fallback.

#### Scenario: RFC duplicado

- GIVEN un consultorio ya registrado con un RFC
- WHEN otro visitante intenta registrarse con el mismo RFC
- THEN se rechaza con mensaje claro y no se crea nada

#### Scenario: Correo, NombreComercial o RazonSocial duplicados

- GIVEN un correo, NombreComercial o RazonSocial ya existente
- WHEN se envía el registro
- THEN se rechaza con mensaje claro del campo duplicado

### Requirement: Alta transaccional

El sistema SHALL crear `DatosInstitucion` + `DatosFiscales` + `Usuario` en UNA sola transacción. Si cualquiera falla, MUST hacer rollback total y MUST NOT dejar consultorio parcial. Un consultorio nuevo SHALL crear exactamente una fila fiscal. La contraseña MUST hashearse (PBKDF2) antes de guardar; MUST NOT enviarse contraseña temporal.

#### Scenario: Alta exitosa

- GIVEN datos válidos y únicos
- WHEN se ejecuta el registro
- THEN quedan creados `DatosInstitucion`, su única fila `DatosFiscales` y el `Usuario` admin activo

#### Scenario: Fallo parcial → rollback

- GIVEN un error al insertar la fila fiscal o el usuario
- WHEN se ejecuta el registro
- THEN la transacción se revierte y no queda ningún registro parcial

### Requirement: Validación de correo y CP

`CorreoContacto` MUST validarse con formato de email. `CodigoPostal` MUST ser entero y SHALL mostrarse a 5 dígitos (formato D5).

#### Scenario: Correo inválido

- GIVEN un correo con formato incorrecto
- WHEN se envía el registro
- THEN se rechaza con mensaje de email inválido

#### Scenario: CP no numérico o distinto de 5 dígitos

- GIVEN un CP no numérico o fuera de 5 dígitos
- WHEN se envía el registro
- THEN se rechaza con mensaje claro
