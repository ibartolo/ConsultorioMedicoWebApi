# Delta para Paquetes Lectura

## ADDED Requirements

### Requirement: Aislamiento por consultorio de la lectura de paquetes

Los endpoints de lectura de paquetes SHALL filtrar por el tenant del usuario autenticado y validar que el paciente/paquete consultado pertenezca al consultorio. Un `pacienteId` o `pacientePaqueteId` de otro consultorio MUST devolver `ModelResponse` con `IsSuccess=false` y lista vacía/sin datos, sin exponer información ajena.

#### Scenario: Paquetes del propio consultorio

- GIVEN un usuario del consultorio 1 y un paciente de su consultorio con paquetes
- WHEN consulta sus paquetes y citas-paquete
- THEN obtiene solo los datos de su consultorio

#### Scenario: Paciente de otro consultorio

- GIVEN un `pacienteId` que pertenece a otro consultorio
- WHEN se llama `api/Historial/PaquetesPorPaciente/{pacienteId}`
- THEN responde `IsSuccess=false` (o lista vacía) sin exponer paquetes ajenos

#### Scenario: PacientePaquete de otro consultorio

- GIVEN un `pacientePaqueteId` de otro consultorio
- WHEN se llama `api/Historial/CitasPorPacientePaquete/{pacientePaqueteId}`
- THEN responde `IsSuccess=false` (o lista vacía) sin exponer citas ajenas

## MODIFIED Requirements

### Requirement: SPs nuevos de solo lectura

El sistema SHALL versionar dos SPs nuevos — `sp_nh_GetPaquetesPorPaciente(@PacienteId, @ConsultorioID)` y `sp_nh_GetCitasPorPacientePaquete(@PacientePaqueteId, @ConsultorioID)` — con patrón idempotente `CREATE` + `IF OBJECT_ID`, y MUST NOT modificar tablas, FKs ni objetos existentes de la BD. Ambos SPs MUST filtrar por el consultorio recibido.

(Previously: los SPs solo recibían el id y no filtraban por consultorio.)

#### Scenario: Script idempotente

- GIVEN un SP `sp_nh_*` ya existente en la BD
- WHEN se ejecuta el script
- THEN el SP no se recrea ni altera (lo detecta `IF OBJECT_ID`)

#### Scenario: Filtrado por consultorio en el SP

- GIVEN paquetes del consultorio 1 y 2
- WHEN se ejecuta `sp_nh_GetPaquetesPorPaciente @PacienteId, @ConsultorioID=1`
- THEN solo devuelve los paquetes del consultorio 1
