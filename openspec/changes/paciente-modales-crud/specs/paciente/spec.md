# Delta para Paciente

> **Nota operativa:** el SP `sp_nh_DeletePaciente` MUST ejecutarse en la BD productiva (Smarterasp) antes del despliegue.

## ADDED Requirements

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
