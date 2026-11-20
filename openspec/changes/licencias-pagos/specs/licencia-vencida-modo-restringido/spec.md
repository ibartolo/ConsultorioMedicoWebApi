# licencia-vencida-modo-restringido Specification

## Purpose

Confinamiento del usuario cuando la licencia de su consultorio está vencida, replicando HelpDesk. El flag `VigenciaVencida` se registra en `TokenCookie` al iniciar sesión (ver `autenticacion-saas`) y el guard global lo usa sin consultas extra por request.

## Requirements

### Requirement: Login no bloqueado por licencia vencida

El login MUST NOT bloquearse por licencia vencida. Un usuario con credenciales válidas SHALL entrar en modo restringido.

#### Scenario: Login con licencia vencida

- GIVEN un usuario con consultorio vencido y credenciales válidas
- WHEN inicia sesión
- THEN entra en modo restringido (el login no se bloquea)

### Requirement: Confinamiento global y allowlist

Mientras `VigenciaVencida == true`, el guard SHALL permitir SOLO `Licencia`, `Home/LogOut` y recursos estáticos (CSS, Scripts, Content, imágenes). Toda otra ruta MUST redirigir a `Licencia`. El guard MUST NOT generar bucle de redirección infinito.

#### Scenario: Ruta no permitida

- GIVEN una sesión con `VigenciaVencida = true`
- WHEN solicita una ruta distinta de `Licencia`/`Home/LogOut`
- THEN redirige a `Licencia`

#### Scenario: Ruta permitida

- GIVEN una sesión restringida
- WHEN solicita `Licencia` o `Home/LogOut`
- THEN se permite sin redirección

#### Scenario: Recursos estáticos

- GIVEN una sesión restringida
- WHEN se solicita un recurso estático
- THEN se sirve sin redirección, para que la vista no se rompa

### Requirement: Aviso visible en Licencia

`Licencia` MUST mostrar un aviso claro de licencia vencida con la `FechaVigenciaFin` del consultorio del usuario e instrucciones de qué hacer.

#### Scenario: Aviso

- GIVEN una sesión restringida en `Licencia`
- WHEN se renderiza la vista
- THEN muestra el aviso con la fecha de vencimiento e instrucciones

### Requirement: Menú de navegación limitado

En modo restringido, el menú de navegación MUST ocultarse o limitarse para que no parezca navegable.

#### Scenario: Menú limitado

- GIVEN una sesión con `VigenciaVencida = true`
- WHEN se renderiza el layout
- THEN la navegación queda oculta o limitada

### Requirement: Renovación y re-login

Tras renovar (subir comprobante → extensión provisional; confirmación manual → +1 año), el usuario MUST volver a iniciar sesión para salir del modo restringido, porque el flag vive en la cookie.

#### Scenario: Re-login

- GIVEN un usuario restringido cuya licencia se renovó
- WHEN cierra sesión y vuelve a iniciar
- THEN el flag se recalcula y se restablece el acceso normal

### Requirement: Retrocompatibilidad de sesiones previas

Las sesiones abiertas antes de este cambio no tienen el flag en su cookie. El sistema MUST tratarlas como no restringidas hasta que el usuario vuelva a iniciar sesión.

#### Scenario: Sesión previa sin flag

- GIVEN una cookie de sesión emitida antes de este cambio (sin `VigenciaVencida`)
- WHEN el usuario navega
- THEN no se aplica el modo restringido (el flag ausente se ignora)
