# Delta para Paciente

## ADDED Requirements

### Requirement: Aislamiento por consultorio del módulo Paciente

El módulo Paciente SHALL incorporar `ConsultorioID` y filtrar por el tenant del usuario autenticado (nunca del cliente). Los endpoints `GET /api/v1/paciente`, `GET /api/v1/paciente/{id}`, `POST /api/v1/paciente/guardar` y `Eliminar/{id}` MUST filtrar por el consultorio y validar pertenencia. Un `{id}` de otro consultorio MUST devolver `ModelResponse` con `IsSuccess=false` y mensaje claro, sin exponer el dato.

#### Scenario: Listado aislado

- GIVEN pacientes de dos consultorios
- WHEN el usuario lista
- THEN solo ve los de su consultorio

#### Scenario: Paciente de otro consultorio

- GIVEN un `Id` que pertenece a otro consultorio
- WHEN se consulta, edita o elimina
- THEN responde `IsSuccess=false` sin filtrar datos

#### Scenario: Guardado asigna consultorio

- GIVEN un paciente nuevo
- WHEN se guarda
- THEN `ConsultorioID` se asigna desde el tenant (no del cliente)

## MODIFIED Requirements

### Requirement: Entidad Paciente

La entidad `Paciente` SHALL representar un registro de `dbo.Paciente` con campos: `Id, ConsultorioID, Nombre, ApellidoPaterno, ApellidoMaterno, Email, FechaAlta, TelefonoCel, Comentarios, Estatus, Edad, Sexo, FechaNacimiento, CreatedBY, CreatedDT, UpdatedBY, UpdatedDT`. La entidad MUST usar Factory Method (constructor privado + `Create()`) y Fluent Interface (métodos `Set…` que retornan `this`), con propiedades de solo lectura. Cada `Set…` MUST validar y lanzar `ArgumentException` ante datos inválidos.

(Previously: la entidad no incluía el campo `ConsultorioID`.)

#### Scenario: Construcción válida con Fluent Interface

- GIVEN un conjunto de datos de paciente válidos
- WHEN se invoca `Paciente.Create().SetId(1).SetNombre("Ana").SetApellidoPaterno("López")`
- THEN se retorna la misma instancia (`this`) en cada `Set…`
- AND las propiedades quedan con sus valores y son de solo lectura

#### Scenario: Validación en el método Set

- GIVEN un nombre vacío o nulo
- WHEN se invoca `SetNombre("")`
- THEN el método lanza `ArgumentException`

### Requirement: Anexos del paciente

El sistema SHALL permitir subir anexos desde el modal **Anexos** del paciente. Cada anexo MUST pertenecer a un paciente (`PacienteID`) y MAY asociarse opcionalmente a una consulta (`ConsultaID`). Los formatos permitidos SHALL ser PDF, PNG, JPG y JPEG; el PDF MUST tener un máximo de 5 MB y las imágenes un máximo de 2 MB. El nombre original MUST conservarse en `Anexo.Nombre` (máximo 150 caracteres) y la ruta parcial MUST guardarse en `Anexo.Ruta` con el patrón `/Attachment/{consultorioId}/{pacienteId}/[{consultaId}/]{guid}.{ext}`. El archivo físico SHALL almacenarse en la WebApi bajo `Attachment/{consultorioId}/`. Todo list/upload/download MUST pasar por API con validación de tenant; el acceso directo a la carpeta MUST estar bloqueado.

(Previously: la ruta era `/Attachment/{pacienteId}/...` sin segmento de consultorio y sin validación de tenant.)

#### Scenario: Subida de un PDF válido

- GIVEN un paciente y un PDF de hasta 5 MB
- WHEN el usuario lo sube desde el modal Anexos
- THEN se registra el anexo con el nombre original y la ruta `/Attachment/{consultorioId}/{pacienteId}/{guid}.pdf`, y el archivo queda bajo `Attachment/{consultorioId}/`

#### Scenario: Anexo de otro consultorio

- GIVEN un intento de listar, subir o descargar un anexo de otro consultorio
- WHEN se realiza la operación
- THEN se rechaza con error controlado (`IsSuccess=false`) sin exponer el archivo

#### Scenario: Archivo rechazado

- GIVEN un archivo con formato no permitido o que excede el tamaño máximo
- WHEN el usuario intenta subirlo
- THEN el sistema muestra un error y no registra el anexo

#### Scenario: Comentarios largos

- GIVEN un anexo con comentarios de hasta 1500 caracteres
- WHEN el usuario guarda
- THEN el comentario se conserva completo
