# Especificación: Autenticación SaaS

## Purpose

Login por `UserName` con PBKDF2 (portado de HelpDesk), verificación del hash en C#, rehash transparente de passwords legados en texto plano, y validación de `Estatus = 1`. Sin 2FA. Login sin branding.

## Requirements

### Requirement: Cryptography PBKDF2

El sistema SHALL portar la clase `Cryptography` de HelpDesk con PBKDF2 en el formato `PBKDF2$iteraciones$salt$hash`, con 10000 iteraciones, salt de 16 bytes y hash de 32 bytes. `Usuario.Password` SHALL pasar de `nvarchar(50)` a `nvarchar(250)`.

#### Scenario: Hash generado

- GIVEN una contraseña nueva
- WHEN se hashea
- THEN se produce `PBKDF2$10000$<salt>$<hash>` en los tamaños definidos

### Requirement: Login por UserName con verificación de hash

El login MUST autenticar por `UserName` verificando el hash en C#. MUST validar `Estatus = 1`. Al éxito SHALL emitir el token OAuth2 (claim `empresaId`) y la cookie de sesión.

#### Scenario: Login exitoso

- GIVEN un usuario activo con hash PBKDF2 correcto
- WHEN ingresa `UserName` y contraseña
- THEN se autentica y se establece sesión/token

#### Scenario: Usuario inactivo

- GIVEN un usuario con `Estatus = 0`
- WHEN intenta iniciar sesión
- THEN se rechaza con mensaje claro

#### Scenario: Credenciales inválidas

- GIVEN `UserName` o contraseña incorrectos
- WHEN intenta iniciar sesión
- THEN se rechaza con mensaje genérico

### Requirement: Rehash transparente de legado

Si el `Password` almacenado es texto plano legado (no inicia con `PBKDF2$`), el sistema SHALL verificar en texto plano y, al primer login exitoso, MUST rehashear y persistir transparentemente.

#### Scenario: Rehash en primer login

- GIVEN un usuario legado con password en texto plano
- WHEN inicia sesión correctamente
- THEN se reemplaza el password por el hash PBKDF2 sin intervención del usuario

### Requirement: Hash en creación y edición de usuarios

Los usuarios creados o editados desde la pantalla `Usuario` SHALL hashear la contraseña. Si en edición el campo password viene vacío, el sistema MUST conservar el password actual.

#### Scenario: Edición sin password

- GIVEN la edición de un usuario con password en blanco
- WHEN se guarda
- THEN se conserva el password actual (no se sobreescribe)

#### Scenario: Creación con password

- GIVEN un usuario nuevo con password
- WHEN se guarda
- THEN se almacena el hash PBKDF2

### Requirement: Login sin branding

La pantalla de login MUST NOT mostrar logo ni nombre de institución.

#### Scenario: Login neutro

- GIVEN la pantalla de login
- WHEN se renderiza
- THEN no muestra logo ni nombre de institución

### Requirement: Sin 2FA

El sistema MUST NOT implementar segundo factor de autenticación en esta entrega.

#### Scenario: Sin segundo factor

- GIVEN un login exitoso
- WHEN se revisa el flujo
- THEN no se solicita ningún código adicional
