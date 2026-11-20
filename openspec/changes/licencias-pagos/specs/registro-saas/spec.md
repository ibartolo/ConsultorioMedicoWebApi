# Delta for Registro SaaS

## MODIFIED Requirements

### Requirement: Alta transaccional

El sistema SHALL crear `DatosInstitucion` + `DatosFiscales` + `Usuario` en UNA sola transacción y, dentro de la misma, SHALL inicializar la licencia de prueba (`sp_nh_SetLicenciaInicial`: `FechaVigenciaInicio=hoy`, `FechaVigenciaFin=hoy+60 días`, `EsPeriodoPrueba=1`, `Plan=Basico`, `Estatus=1`). Si cualquiera falla, MUST hacer rollback total y MUST NOT dejar consultorio parcial. Un consultorio nuevo SHALL crear exactamente una fila fiscal. La contraseña MUST hashearse (PBKDF2) antes de guardar; MUST NOT enviarse contraseña temporal. El correo de bienvenida MUST enviarse de forma best-effort y MUST NOT incluir la contraseña ni el hash.
(Previously: el alta no inicializaba licencia/trial; el consultorio arrancaba sin vigencia.)

#### Scenario: Alta exitosa

- GIVEN datos válidos y únicos
- WHEN se ejecuta el registro
- THEN quedan creados `DatosInstitucion` (con licencia trial de 60 días), su única fila `DatosFiscales` y el `Usuario` admin activo

#### Scenario: Fallo parcial → rollback

- GIVEN un error al insertar la fila fiscal, el usuario o la licencia
- WHEN se ejecuta el registro
- THEN la transacción se revierte y no queda ningún registro parcial

#### Scenario: Licencia de prueba configurada

- GIVEN un alta exitosa
- WHEN se revisa el consultorio
- THEN `EsPeriodoPrueba=1`, `Plan=Basico`, `Estatus=1`, vigencia hoy + 60 días
