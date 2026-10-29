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

El endpoint SHALL crear (`Id=0`) o actualizar (`Id>0`) un paciente vía `PacienteAplicacion.Guardar()` → `SaveOrUpdatePaciente` → `ModelResponse<Paciente>` (entidad con `Id` ya seteado). El SP SHALL recibir solo los campos del Request y SHALL calcular internamente `Edad` (desde `FechaNacimiento`), `FechaAlta` (al crear) y auditoría (`CreatedBY/DT` al crear; `UpdatedBY/DT` al actualizar).

#### Scenario: Crear paciente

- GIVEN `Id=0` y Request válido
- WHEN se llama `POST /api/v1/paciente/guardar`
- THEN responde 200 con `IsSuccess=true`, `Message="Paciente creado exitosamente"` y `Response` es la entidad `Paciente` con el nuevo `Id`

#### Scenario: Actualizar paciente

- GIVEN `Id>0` existente y Request válido
- WHEN se llama `POST /api/v1/paciente/guardar`
- THEN responde 200 con `IsSuccess=true` y `Response` es la entidad `Paciente` con el `Id` actualizado

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
- THEN Infraestructura ejecuta `SaveOrUpdatePaciente` y Aplicacion retorna `ModelResponse<Paciente>` con el `Id` ya seteado

### Requirement: Alta y edición en modal

El módulo Pacientes MUST ofrecer alta y edición mediante un modal ancho y largo que renderiza el formulario como vista parcial reutilizable. Los botones "Nuevo Paciente" y Editar MUST abrir el modal (sin navegar). La ruta `Paciente/Nuevo/{id}` SHALL seguir funcionando como wrapper.

#### Scenario: Alta y edición desde modal

- GIVEN el listado de Pacientes
- WHEN se pulsa "Nuevo Paciente" o Editar en una fila
- THEN se abre el modal con el formulario (en blanco en alta; con datos en edición)

### Requirement: Carga de datos para edición

En edición (Id>0), el modal MUST cargar los datos frescos del paciente por Id vía la acción MVC JSON `ConsultarPacientePorId`, incluyendo `NombreCompleto` y `Edad`.

#### Scenario: Carga exitosa

- GIVEN un paciente con Id>0
- WHEN se abre el modal en edición
- THEN el formulario se rellena con datos frescos

#### Scenario: Paciente no encontrado

- GIVEN un Id inexistente
- WHEN se consulta `ConsultarPacientePorId`
- THEN se muestra un error y el modal no se rellena

### Requirement: Guardado con validación y confirmación

Al pulsar Guardar, el sistema MUST validar con jQuery Validate y pedir confirmación SweetAlert ("¿Confirmás guardar los cambios?"). Si cancela, MUST NOT guardar; si confirma, MUST enviar POST `GuardarOActualizarPaciente` (form-data).

#### Scenario: Validación fallida

- GIVEN un formulario inválido
- WHEN se pulsa Guardar
- THEN se muestran los errores y no se guarda

#### Scenario: Confirmación cancelada

- GIVEN un formulario válido
- WHEN el usuario cancela
- THEN no se guarda

#### Scenario: Confirmación aceptada

- GIVEN un formulario válido
- WHEN el usuario confirma
- THEN se guarda el paciente

### Requirement: Refresco de fila afectada

Tras guardar con éxito, el sistema MUST actualizar SOLO la fila afectada (o insertarla en alta) y cerrar el modal, SIN recargar todos los registros. MUST NOT usar `window.location` ni `ajax.reload()`.

#### Scenario: Edición refresca fila

- GIVEN un paciente editado y guardado
- WHEN se cierra el modal
- THEN solo esa fila se actualiza

#### Scenario: Alta inserta fila

- GIVEN un paciente nuevo guardado
- WHEN se cierra el modal
- THEN se inserta una nueva fila

### Requirement: Eliminación lógica de Paciente

Cada fila MUST ofrecer Eliminar con confirmación SweetAlert ("¿Estás seguro que quieres eliminar a este paciente?"). La baja es lógica (Estatus=0): el paciente desaparece del listado sin borrarse de la BD. Tras eliminar, MUST quitar SOLO esa fila.

#### Scenario: Confirmación cancelada

- GIVEN la confirmación abierta
- WHEN el usuario cancela
- THEN no se elimina

#### Scenario: Eliminación confirmada

- GIVEN la confirmación aceptada
- WHEN la eliminación es exitosa
- THEN la fila se quita y el paciente queda con Estatus=0

### Requirement: Backend de eliminación

El sistema SHALL implementar la baja lógica con: SP `sp_nh_DeletePaciente` (CREATE + IF OBJECT_ID, `UPDATE Estatus=0`), endpoint WebApi `[HttpPost, Route("Eliminar/{id:int}")]` → `ModelResponse<bool>`, y acción MVC `[HttpPost] EliminarPaciente(long id)` → JSON (servicios y DAL en ambas capas). Si `id<=0`, MUST retornar `IsSuccess=false`.

#### Scenario: Eliminación exitosa

- GIVEN un id válido
- WHEN se invoca Eliminar
- THEN responde `IsSuccess=true` y Estatus pasa a 0

#### Scenario: Id inválido o inexistente

- GIVEN `id<=0` o un id inexistente
- WHEN se invoca Eliminar
- THEN responde `IsSuccess=false`

### Requirement: No regresión

El listado `GetAllPacienteByEstatus`, la validación `PacienteValidator` (MVC y WebApi) y las rutas existentes MUST seguir funcionando.

#### Scenario: Listado y validación intactos

- GIVEN la migración al modal
- WHEN se carga el listado y se validan datos
- THEN el listado muestra pacientes activos y la validación rechaza datos inválidos

### Requirement: Acciones del listado de Pacientes

El listado de Pacientes SHALL mostrar por fila las acciones **Expediente clínico**, **Anexos**, **Editar** y **Eliminar**. "Expediente clínico" MUST navegar a `/Historial/Expediente/{id}`; "Anexos" MUST abrir el modal de anexos del paciente; "Editar" MUST abrir el modal de edición; "Eliminar" MUST aplicar baja lógica.

#### Scenario: Acciones disponibles en la fila

- GIVEN un paciente listado
- WHEN se inspecciona su fila
- THEN se muestran las acciones Expediente, Anexos, Editar y Eliminar

### Requirement: Anexos del paciente

El sistema SHALL permitir subir anexos desde el modal **Anexos** del paciente. Cada anexo MUST pertenecer a un paciente (`PacienteID`) y MAY asociarse opcionalmente a una consulta (`ConsultaID`). Los formatos permitidos SHALL ser PDF, PNG, JPG y JPEG; el PDF MUST tener un máximo de 5 MB y las imágenes un máximo de 2 MB. El nombre original MUST conservarse en `Anexo.Nombre` (máximo 150 caracteres) y la ruta parcial MUST guardarse en `Anexo.Ruta` con el patrón `/Attachment/{pacienteId}/[{consultaId}/]{guid}.{ext}`. El archivo físico SHALL almacenarse en la WebApi bajo `Attachment/`.

#### Scenario: Subida de un PDF válido

- GIVEN un paciente y un PDF de hasta 5 MB
- WHEN el usuario lo sube desde el modal Anexos
- THEN se registra el anexo con el nombre original y la ruta parcial, y el archivo queda en `Attachment/`

#### Scenario: Archivo rechazado

- GIVEN un archivo con formato no permitido o que excede el tamaño máximo
- WHEN el usuario intenta subirlo
- THEN el sistema muestra un error y no registra el anexo

#### Scenario: Comentarios largos

- GIVEN un anexo con comentarios de hasta 1500 caracteres
- WHEN el usuario guarda
- THEN el comentario se conserva completo

### Requirement: Descarga de anexos

El listado de anexos SHALL permitir descargar cada archivo mediante un endpoint autenticado. La descarga MUST conservar el nombre original del archivo.

#### Scenario: Descarga de un anexo

- GIVEN un anexo registrado
- WHEN el usuario pulsa el botón de descarga
- THEN el navegador descarga el archivo con su nombre original
