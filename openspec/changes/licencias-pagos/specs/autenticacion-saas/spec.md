# Delta for Autenticación SaaS

## ADDED Requirements

### Requirement: Registro del estado de vigencia en la sesión

`TokenCookie` SHALL incorporar el campo `VigenciaVencida`. Al iniciar sesión, el MVC SHALL calcularlo desde la licencia del consultorio (`FechaVigenciaFin`/`EsPeriodoPrueba`) y guardarlo en la cookie, de modo que el guard global lo conozca sin consultas extra por request.

#### Scenario: Cookie con flag vencido

- GIVEN un usuario cuyo consultorio tiene `FechaVigenciaFin` vencida
- WHEN inicia sesión correctamente
- THEN `TokenCookie.VigenciaVencida = true`

#### Scenario: Cookie con flag vigente

- GIVEN un usuario cuyo consultorio está vigente
- WHEN inicia sesión correctamente
- THEN `TokenCookie.VigenciaVencida = false`

### Requirement: Login no bloqueado por licencia vencida

El login MUST NOT bloquearse por licencia vencida: con credenciales válidas y `Estatus=1`, el sistema SHALL autenticar y emitir el token OAuth2 y la cookie con `VigenciaVencida`. La restricción se aplica después, vía guard (modo restringido).

#### Scenario: Login con licencia vencida

- GIVEN credenciales válidas de un usuario con consultorio vencido
- WHEN inicia sesión
- THEN se autentica y recibe cookie con `VigenciaVencida = true`
