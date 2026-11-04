# Especificación: Aislamiento Multi-Tenant

## Purpose

Aislamiento de datos por consultorio. Cada consultorio ve y gestiona solo lo suyo. El tenant se resuelve del usuario autenticado y nunca se acepta del cliente. SaaS básico SIN roles ni permisos: N usuarios por consultorio; todos pueden ver y hacer todo dentro de su consultorio.

## Requirements

### Requirement: Resolución del tenant desde el usuario autenticado

El tenant (consultorio) SHALL resolverse exclusivamente del usuario autenticado: la WebApi desde el claim `empresaId` (vía `BaseController.ObtenerEmpresaIdDesdeClaim()`), el MVC desde `TokenCookie.EmpresaID`. El sistema MUST NOT aceptar el tenant/`ConsultorioID`/`empresaId` proveniente del cliente (query, body o ruta). `DatosInstitucion.Id` es el id del consultorio y `Usuario.CompaniaID` es la FK.

#### Scenario: Resolución WebApi

- GIVEN un token con claim `empresaId=2`
- WHEN la WebApi atiende una petición
- THEN usa el consultorio 2 para filtrar y validar

#### Scenario: Resolución MVC

- GIVEN una cookie `TokenCookie.EmpresaID=2`
- WHEN el MVC llama a la WebApi
- THEN propaga el consultorio 2 como tenant del usuario

#### Scenario: Intento de suplantación de tenant

- GIVEN un cliente que envía `empresaId`/`ConsultorioID`=3 en el body
- WHEN la petición llega al backend
- THEN el backend ignora ese valor y usa el tenant del usuario autenticado

### Requirement: Columnas ConsultorioID

Las tablas de dominio SHALL incorporar `ConsultorioID` (FK a `DatosInstitucion`) con backfill a 1 y luego NOT NULL: Paciente, Cita, Consulta, Anexo, PacientePaquete, PagoPaquete, CitaPaquete, Tratamiento, TratamientoPaquete, Paquete, TipoConsulta.

#### Scenario: Columna presente

- GIVEN la migración aplicada
- WHEN se inspecciona el esquema
- THEN cada tabla tiene `ConsultorioID` NOT NULL con FK a `DatosInstitucion`

### Requirement: Filtrado y validación de pertenencia

Todo endpoint y pantalla (Dashboard, Historial, Paciente, Cita, Consulta, Anexo, Usuario, DatosInstitucion/DatosFiscales) MUST filtrar por el tenant y MUST validar que el ID consultado pertenezca al consultorio. Un ID de otro consultorio MUST devolver error controlado (`ModelResponse` con `IsSuccess=false`) SIN filtrar ni exponer datos.

#### Scenario: Listado aislado

- GIVEN dos consultorios con datos
- WHEN un usuario del consultorio 1 lista un recurso
- THEN solo ve registros del consultorio 1

#### Scenario: ID de otro consultorio

- GIVEN un ID que pertenece al consultorio 2
- WHEN un usuario del consultorio 1 lo consulta o intenta editarlo
- THEN responde `IsSuccess=false` con mensaje claro, sin exponer el dato ajeno

### Requirement: Catálogos por consultorio

`Tratamiento`, `Paquete` y `TipoConsulta` SHALL filtrar e insertar por consultorio. Un consultorio nuevo SHALL iniciar con catálogos vacíos (sin seeds).

#### Scenario: Catálogo aislado

- GIVEN tratamientos de los consultorios 1 y 2
- WHEN el consultorio 2 abre el catálogo de Tratamientos
- THEN solo ve los suyos

#### Scenario: Consultorio nuevo sin catálogos

- GIVEN un consultorio recién creado
- WHEN abre Tratamientos/Paquetes/TipoConsulta
- THEN las listas están vacías

### Requirement: Consultorio 1 y backfill

El consultorio 1 SHALL ser "Dra. Tania Guadalupe Rios Bartolo" (`DatosInstitucion.Id = 1`). Todo dato existente SHALL backfillearse a `ConsultorioID = 1`. El usuario `admin` SHALL conservarse como `admin`; el usuario `ivanbartolo` (`CompaniaID` NULL) SHALL asignarse al consultorio 1.

#### Scenario: Backfill a consultorio 1

- GIVEN la migración ejecutada sobre el clon
- WHEN se revisan los datos
- THEN todos los registros existentes tienen `ConsultorioID=1` y `ivanbartolo.CompaniaID=1`

#### Scenario: Fiscal del consultorio 1 vacío

- GIVEN el consultorio 1 (Dra. Tania)
- WHEN se inspecciona `DatosFiscales`
- THEN el consultorio 1 NO tiene fila fiscal (queda vacío), sin fila fiscal global

### Requirement: DatosInstitucion y DatosFiscales por tenant

`DatosInstitucion` y `DatosFiscales` SHALL resolver el tenant actual; `DatosFiscales` MUST NOT seguir usando "Unico" global. La pantalla `Usuario` SHALL filtrar por consultorio y SHALL asignar `CompaniaID` automáticamente; el cliente MUST NOT poder elegir consultorio.

#### Scenario: Institución del tenant

- GIVEN un usuario del consultorio 2
- WHEN abre Datos de la Institución
- THEN ve y edita solo `DatosInstitucion` del consultorio 2

#### Scenario: Alta de usuario asigna consultorio

- GIVEN un admin del consultorio 2 crea un usuario
- WHEN guarda
- THEN `CompaniaID` se asigna a 2 automáticamente

### Requirement: Reubicación de archivos al consultorio 1

En la migración, el logo existente `Attachment/institucion/...` y los anexos existentes SHALL reubicarse bajo el consultorio 1 (`Attachment/1/...`), actualizando `DatosInstitucion.LogoRuta` y `Anexo.Ruta`. El acceso directo a la carpeta de archivos MUST quedar bloqueado; todo download/list/upload MUST pasar por API con validación de tenant.

#### Scenario: Logo reubicado

- GIVEN el logo institucional en `Attachment/institucion/...`
- WHEN se ejecuta la migración
- THEN el archivo queda bajo `Attachment/1/...` y `LogoRuta` se actualiza

#### Scenario: Acceso directo bloqueado

- GIVEN una URL directa a un archivo de `Attachment/`
- WHEN un usuario no autenticado o de otro tenant intenta acceder
- THEN el acceso se deniega; solo se accede vía API con validación de tenant

### Requirement: Alcance SaaS y entornos

Solo `develop` y `main` con el clon `db_9c7990_consultoriodesitst` SHALL ser SaaS. Las ramas/BD `-dermatologia` MUST permanecer intactas. No existe BD productiva SaaS aún.

#### Scenario: Aislamiento de ramas

- GIVEN el cambio implementado
- WHEN se revisan el código y las migraciones
- THEN no se tocan ramas ni BD de `-dermatologia`

### Requirement: Branding por consultorio (sidebar)

Después de autenticar, la barra lateral (sidebar) SHALL mostrar el nombre y logo de `DatosInstitucion` del tenant actual.

#### Scenario: Sidebar con branding del tenant

- GIVEN un usuario autenticado del consultorio 2
- WHEN carga la aplicación
- THEN el sidebar muestra nombre/logo del consultorio 2
