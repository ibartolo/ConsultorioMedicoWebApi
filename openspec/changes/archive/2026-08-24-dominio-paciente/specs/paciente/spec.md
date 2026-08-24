# Especificación: Dominio Paciente

## Purpose

Fase 1 del sistema (greenfield): dominio Paciente únicamente. Define la entidad, value objects, DTOs de request/response, el sobre público `ModelResponse<T>`, los 3 SPs y los 3 endpoints REST. Acceso a datos estrictamente ADO.NET (sin ORM). Fuente de verdad: `espesificaiones.md`, `espesificaciones2.md`, `espesificaciones3.md` — **`espesificaciones3.md` prevalece en conflictos**.

## Requirements

### Requirement: Entidad Paciente

La entidad `Paciente` SHALL representar un registro de `dbo.Paciente` con campos: `Id, Nombre, ApellidoPaterno, ApellidoMaterno, Email, FechaAlta, TelefonoCel, Comentarios, Estatus, Edad, Sexo, FechaNacimiento, CreatedBY, CreatedDT, UpdatedBY, UpdatedDT`. La entidad MUST usar Factory Method (constructor privado + `Create()`) y Fluent Interface (métodos `Set…` que retornan `this`), con propiedades de solo lectura. Cada `Set…` MUST validar y lanzar `ArgumentException` ante datos inválidos.

#### Scenario: Construcción válida con Fluent Interface

- GIVEN un conjunto de datos de paciente válidos
- WHEN se invoca `Paciente.Create().SetId(1).SetNombre("Ana").SetApellidoPaterno("López")`
- THEN se retorna la misma instancia (`this`) en cada `Set…`
- AND las propiedades quedan con sus valores y son de solo lectura

#### Scenario: Validación en el método Set

- GIVEN un nombre vacío o nulo
- WHEN se invoca `SetNombre("")`
- THEN el método lanza `ArgumentException`

### Requirement: ValueObjects Email, Telefono y Sexo

Los value objects SHALL encapsular y validar sus reglas en la capa Dominio:

| VO | Tipo | Reglas |
|----|------|--------|
| Email | string? | Formato válido; máx. 50 caracteres |
| Telefono | string? | Máx. 100 caracteres |
| Sexo | string? | Máx. 15; solo `"Masculino"` o `"Femenino"` |

`Sexo` MUST ser string (validado), **no** enum.

#### Scenario: Email inválido

- GIVEN un email con formato incorrecto (ej. `"ana@"`)
- WHEN se construye el value object Email
- THEN se lanza `ArgumentException`

#### Scenario: Sexo fuera de catálogo

- GIVEN un valor `"Otro"`
- WHEN se valida Sexo
- THEN se rechaza; solo `"Masculino"`/`"Femenino"` son válidos

### Requirement: Sobre público ModelResponse\<T\>

TODO endpoint SHALL retornar `ModelResponse<T>` con la forma `{ IsSuccess: bool, Message: string, Response: T }`. `IsSuccess` indica éxito; `Message` describe el resultado; `Response` transporta el dato (`GetPacientesResponse`, `PacienteResponse` o `int`).

#### Scenario: Respuesta exitosa

- GIVEN una operación correcta
- WHEN el Controller retorna el resultado
- THEN `IsSuccess` es `true` y `Response` contiene el dato esperado

#### Scenario: Respuesta de error controlado

- GIVEN un error de negocio (ej. paciente no encontrado)
- WHEN el Controller retorna el resultado
- THEN `IsSuccess` es `false`, `Response` es `null` y `Message` describe el error

### Requirement: DTO GuardarPacienteRequest

El DTO de entrada `GuardarPacienteRequest` SHALL contener únicamente: `Id (int), Nombre (string), ApellidoPaterno (string), ApellidoMaterno (string?), Email (string?), TelefonoCel (string?), Comentarios (string?), Sexo (string?), FechaNacimiento (DateTime?), Estatus (bool)`. MUST validarse con DataAnnotations:

| Campo | Validación |
|-------|-----------|
| Nombre | `[Required]`, `[MaxLength(50)]` |
| ApellidoPaterno | `[Required]`, `[MaxLength(50)]` |
| ApellidoMaterno | `[MaxLength(50)]` |
| Email | `[EmailAddress]`, `[MaxLength(50)]` |
| TelefonoCel | `[MaxLength(100)]` |
| Comentarios | `[MaxLength(1000)]` |
| Sexo | `[MaxLength(15)]` ("Masculino"/"Femenino") |
| Estatus | `[Required]` |

`Edad`, `FechaAlta` y campos de auditoría MUST NOT estar en el Request.

#### Scenario: Request válido

- GIVEN `Nombre` y `ApellidoPaterno` con valor y `Estatus` presente
- WHEN se valida `GuardarPacienteRequest`
- THEN `ModelState.IsValid` es `true`

#### Scenario: Request sin Nombre

- GIVEN `Nombre` vacío
- WHEN se valida el Request
- THEN `ModelState.IsValid` es `false` con mensaje "Nombre es requerido"

### Requirement: DTOs PacienteResponse y GetPacientesResponse

`PacienteResponse` SHALL exponer: `Id, Nombre, ApellidoPaterno, ApellidoMaterno, NombreCompleto, Sexo, Edad, TelefonoCel, Email, Estatus, FechaNacimiento, Comentarios`. `GetPacientesResponse` SHALL contener `Pacientes (List<PacienteResponse>)`. El resultado de la operación se transmite en el sobre `ModelResponse<T>`, no dentro del DTO.

#### Scenario: NombreCompleto compuesto

- GIVEN `Nombre`, `ApellidoPaterno` y `ApellidoMaterno` presentes
- WHEN se mapea a `PacienteResponse`
- THEN `NombreCompleto` es la concatenación de los tres campos

### Requirement: GET /api/v1/paciente

El endpoint SHALL retornar todos los pacientes vía `PacienteAplicacion.ObtenerTodos()` → `GetAllPacientes` (sin parámetros) → `Mapp` → `ModelResponse<GetPacientesResponse>`.

#### Scenario: Lista con pacientes

- GIVEN pacientes existentes en BD
- WHEN se llama `GET /api/v1/paciente`
- THEN responde 200 con `IsSuccess=true`, `Message="Pacientes obtenidos correctamente"` y la lista poblada

#### Scenario: Lista vacía

- GIVEN la tabla sin registros
- WHEN se llama `GET /api/v1/paciente`
- THEN responde 200 con `IsSuccess=true` y `Pacientes` vacío

### Requirement: GET /api/v1/paciente/{id}

El endpoint SHALL retornar un paciente por `{id}` vía `GetPacienteById(@Id)` → `Mapp` → `ModelResponse<PacienteResponse>`.

#### Scenario: Paciente encontrado

- GIVEN un paciente con `Id=5`
- WHEN se llama `GET /api/v1/paciente/5`
- THEN responde 200 con `IsSuccess=true` y `Response.Id=5`

#### Scenario: Paciente no encontrado

- GIVEN un `Id` inexistente (ej. `9999`)
- WHEN se llama `GET /api/v1/paciente/9999`
- THEN responde 404 con `IsSuccess=false`, `Message="Paciente no encontrado"` y `Response=null`

#### Scenario: Id inválido

- GIVEN un `id` no numérico o ≤ 0
- WHEN se llama el endpoint
- THEN responde 400 por validación

### Requirement: POST /api/v1/paciente/guardar

El endpoint SHALL crear (`Id=0`) o actualizar (`Id>0`) un paciente vía `PacienteAplicacion.Guardar()` → `SaveOrUpdatePaciente` → `ModelResponse<int>` (Id retornado). El SP SHALL recibir solo los campos del Request y SHALL calcular internamente `Edad` (desde `FechaNacimiento`), `FechaAlta` (al crear) y auditoría (`CreatedBY/DT` al crear; `UpdatedBY/DT` al actualizar).

#### Scenario: Crear paciente

- GIVEN `Id=0` y Request válido
- WHEN se llama `POST /api/v1/paciente/guardar`
- THEN responde 200 con `IsSuccess=true`, `Message="Paciente creado exitosamente"` y `Response` igual al nuevo Id

#### Scenario: Actualizar paciente

- GIVEN `Id>0` existente y Request válido
- WHEN se llama `POST /api/v1/paciente/guardar`
- THEN responde 200 con `IsSuccess=true` y `Response` igual al Id actualizado

#### Scenario: Validación fallida

- GIVEN un Request sin `Nombre` o con `Email`/`Sexo` inválidos
- WHEN se llama el endpoint
- THEN responde 400 y no invoca el SP

### Requirement: Flujo de datos por endpoint

El flujo SHALL ser `Controller → Aplicacion → Infraestructura (ADO.NET → SP) → Mapp → entidad → ModelResponse<T>`; la Infraestructura SHALL devolver datos crudos (sin lógica de negocio) y `Mapp` SHALL mapear a entidades/DTOs. Los errores SHALL manejarse a nivel de Controller.

#### Scenario: Flujo de obtención

- GIVEN una petición `GET`
- WHEN el Controller delega en Aplicacion
- THEN Aplicacion llama Infraestructura→SP, mapea con `Mapp` y retorna `ModelResponse<T>`

#### Scenario: Flujo de guardado

- GIVEN una petición `POST`
- WHEN el Controller delega en Aplicacion
- THEN Infraestructura ejecuta `SaveOrUpdatePaciente` y Aplicacion retorna `ModelResponse<int>` con el Id
