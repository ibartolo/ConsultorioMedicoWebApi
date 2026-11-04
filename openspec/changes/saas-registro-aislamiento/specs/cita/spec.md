# Delta para Cita

## ADDED Requirements

### Requirement: Aislamiento por consultorio del módulo Cita

El módulo Cita SHALL incorporar `ConsultorioID` y filtrar por el tenant del usuario autenticado. El calendario de Citas MUST pintar solo las citas del consultorio; la eliminación MUST validar pertenencia. Una cita de otro consultorio MUST devolver `ModelResponse` con `IsSuccess=false`, sin filtrar datos.

#### Scenario: Calendario aislado

- GIVEN citas de dos consultorios
- WHEN el usuario carga el calendario
- THEN solo ve las citas de su consultorio

#### Scenario: Cita de otro consultorio

- GIVEN un `Id` de cita de otro consultorio
- WHEN se intenta eliminar
- THEN responde `IsSuccess=false` sin modificar datos

### Requirement: Corrección de FKs de CitaPaquete

El sistema SHALL corregir las FKs incorrectas de `CitaPaquete`: `FK_CitaPaquete_Cita` MUST referenciar `Cita.Id` (no `Paciente`) y `FK_CitaPaquete_PacientePaquete` MUST referenciar `PacientePaquete.Id` (no `Paquete`). La corrección SHALL realizarse mediante migración idempotente antes de habilitar escritura.

#### Scenario: FK corregida

- GIVEN la migración aplicada
- WHEN se inspeccionan las FKs de `CitaPaquete`
- THEN `CitaID` referencia `Cita.Id` y `PacientePaqueteID` referencia `PacientePaquete.Id`

## MODIFIED Requirements

### Requirement: Baja lógica de Cita vía SP

El sistema SHALL exponer el SP `sp_nh_DeleteCita` (`@Id INT`, `@ConsultorioID INT`) que ejecuta `UPDATE dbo.Cita SET Estatus = 0 WHERE Id = @Id AND ConsultorioID = @ConsultorioID`. El SP MUST crearse idempotente (CREATE + IF OBJECT_ID, sin DROP/ALTER). La eliminación MUST ser baja lógica (`Estatus=0`), nunca borrado físico, y MUST estar acotada al consultorio.

(Previously: el SP solo tomaba `@Id` y no filtraba por consultorio.)

#### Scenario: Baja lógica exitosa

- GIVEN una cita con `Id` existente del consultorio autenticado
- WHEN se ejecuta `sp_nh_DeleteCita @Id, @ConsultorioID`
- THEN `Estatus` queda en 0 y el registro persiste en la tabla

#### Scenario: Cita de otro consultorio no se elimina

- GIVEN una cita cuyo `ConsultorioID` no coincide con el del usuario
- WHEN se ejecuta `sp_nh_DeleteCita`
- THEN la cita NO se modifica y el registro permanece activo

### Requirement: Endpoint de eliminación

El sistema SHALL exponer `POST api/Cita/Eliminar/{id:int}` (id por ruta) que recibe el `id` y SHALL retornar `ModelResponse<bool>` (HTTP 200, mensajes en español): `IsSuccess=true` con `Response=true` al completar la baja lógica; `IsSuccess=false` con `Message` de error ante fallo, id inexistente, id de otro consultorio o id inválido (≤ 0). Flujo: Controller → Service → DbWrapper (`sp_nh_DeleteCita` con el `ConsultorioID` del claim).

(Previously: el endpoint no validaba pertenencia al consultorio.)

#### Scenario: Eliminación exitosa

- GIVEN una cita activa del consultorio con `Id>0`
- WHEN se llama `POST api/Cita/Eliminar/{id}`
- THEN responde 200 con `IsSuccess=true` y la cita queda con `Estatus=0`

#### Scenario: Cita de otro consultorio

- GIVEN una cita cuyo `Id` pertenece a otro consultorio
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=false` y `Message` en español

#### Scenario: Error de eliminación

- GIVEN un fallo de BD o id inexistente
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=false` y `Message` en español

#### Scenario: Id inválido

- GIVEN un `id` no numérico o ≤ 0
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=false` y `Message` en español (no 400)
