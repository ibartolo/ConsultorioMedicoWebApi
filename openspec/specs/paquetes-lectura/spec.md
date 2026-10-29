# Especificación: Paquetes Lectura

## Purpose

Solo lectura de los paquetes adquiridos por un paciente y sus citas-tratamientos, expuestos en la Tab 4 "Paquetes" del Expediente Clínico (`Historial/Expediente/{id}`). Replica en solo lectura la pestaña Paquetes del `CatalogoHistorial` WinForms: grid izquierdo de paquetes (`PacientePaquete` JOIN `Paquete`) y, al seleccionar uno, grid derecho de citas-tratamientos (`CitaPaquete` JOIN `Cita` LEFT JOIN `Tratamiento`). Flujo MVC → WebApi → SP (patrón `sp_nh_GetTratamientosPorPaciente`). Sin pagos, sin altas, sin tocar el esquema ni las FKs de la BD.

## Requirements

### Requirement: Grid de paquetes adquiridos del paciente

El sistema SHALL mostrar en la Tab 4 "Paquetes" un grid de solo lectura (`#paquetesTable`) con los paquetes adquiridos por el paciente. Cada fila SHALL exponer el Nombre del paquete. Los datos MUST derivarse de `PacientePaquete` JOIN `Paquete` vía el endpoint `api/Historial/PaquetesPorPaciente/{pacienteId}`.

#### Scenario: Paquetes listados

- GIVEN el expediente del paciente 248, que tiene el paquete "PAQUETE CBX CICRATIZ" (PacientePaquete Id=9, Paquete Id=8)
- WHEN se abre la Tab 4 "Paquetes"
- THEN el grid izquierdo lista una fila con el Nombre "PAQUETE CBX CICRATIZ"

#### Scenario: Paciente sin paquetes

- GIVEN un paciente sin registros en `PacientePaquete`
- WHEN se carga el grid de paquetes
- THEN el grid queda vacío sin error

### Requirement: Grid de citas-tratamientos del paquete

Al hacer click en una fila de paquetes, el sistema SHALL recargar un segundo grid (`#citasPaqueteTable`) con las citas del paquete seleccionado, mostrando FechaCita, Tratamiento y Comentarios. Los datos MUST derivarse de `CitaPaquete` JOIN `Cita` LEFT JOIN `Tratamiento` vía `api/Historial/CitasPorPacientePaquete/{pacientePaqueteId}`. El LEFT JOIN SHALL conservar las citas sin tratamiento (Tratamiento vacío).

#### Scenario: Citas listadas al seleccionar un paquete

- GIVEN el paquete PacientePaquete Id=9 seleccionado
- WHEN el usuario hace click en la fila del paquete
- THEN el grid derecho lista 7 citas (Ids 526–532) con FechaCita, Tratamiento y Comentarios

#### Scenario: Cita sin tratamiento

- GIVEN una cita del paquete sin tratamiento asociado
- WHEN se carga el grid de citas
- THEN la fila se muestra con Tratamiento vacío y no se pierde

#### Scenario: Paquete sin citas

- GIVEN un paquete sin registros en `CitaPaquete`
- WHEN el usuario lo selecciona
- THEN el grid derecho queda vacío sin error

### Requirement: Selección automática del primer paquete

Al cargar la pestaña Paquetes, si el paciente tiene paquetes, el sistema SHALL seleccionar automáticamente el primer paquete y cargar sus citas-tratamientos en el grid derecho. El click en otra fila SHALL cambiar la selección y recargar el grid.

#### Scenario: Primer paquete seleccionado al cargar

- GIVEN un paciente con paquetes
- WHEN se abre la pestaña Paquetes
- THEN el primer paquete queda seleccionado y el grid derecho muestra sus citas-tratamientos

#### Scenario: Cambio de paquete

- GIVEN la pestaña Paquetes con un paquete seleccionado
- WHEN el usuario hace click en otra fila de paquetes
- THEN la selección cambia y el grid derecho se recarga con las citas de ese paquete

### Requirement: Endpoint GET PaquetesPorPaciente

El sistema SHALL exponer `GET api/Historial/PaquetesPorPaciente/{pacienteId}` de solo lectura que retorne `ModelResponse<List<PacientePaqueteLecturaDTO>>` (HTTP 200, mensajes en español, camelCase). Sus datos MUST derivarse del SP nuevo `sp_nh_GetPaquetesPorPaciente`.

#### Scenario: Respuesta exitosa

- GIVEN un `pacienteId` válido con paquetes asociados
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=true` y la lista de paquetes en `Response`

#### Scenario: Paciente sin paquetes

- GIVEN un `pacienteId` sin registros
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=true` y lista vacía

#### Scenario: Id inválido

- GIVEN un `pacienteId` inválido (≤ 0)
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=false` y mensaje en español (no 400)

### Requirement: Endpoint GET CitasPorPacientePaquete

El sistema SHALL exponer `GET api/Historial/CitasPorPacientePaquete/{pacientePaqueteId}` de solo lectura que retorne `ModelResponse<List<CitaPaqueteLecturaDTO>>` (HTTP 200, español, camelCase). Sus datos MUST derivarse del SP nuevo `sp_nh_GetCitasPorPacientePaquete`.

#### Scenario: Respuesta exitosa

- GIVEN un `pacientePaqueteId` válido con citas
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=true` y la lista de citas en `Response`

#### Scenario: Sin citas

- GIVEN un `pacientePaqueteId` sin registros
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=true` y lista vacía

#### Scenario: Id inválido

- GIVEN un `pacientePaqueteId` inválido (≤ 0)
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=false` y mensaje en español (no 400)

### Requirement: DTOs de lectura de paquetes

`PacientePaqueteLecturaDTO` SHALL contener `Id, PacienteID, PaqueteID, PaqueteNombre`. `CitaPaqueteLecturaDTO` SHALL contener `Id, FechaCita, TratamientoNombre, Comentarios, CitaPaqueteID, PacientePaqueteID`. El `Id` de `CitaPaqueteLecturaDTO` SHALL corresponder al Id de la Cita.

#### Scenario: Contratos definidos

- GIVEN una respuesta del endpoint
- WHEN se deserializa en el DTO correspondiente
- THEN las propiedades coinciden con las columnas del SP asociado

### Requirement: SPs nuevos de solo lectura

El sistema SHALL versionar dos SPs nuevos — `sp_nh_GetPaquetesPorPaciente(@PacienteId)` y `sp_nh_GetCitasPorPacientePaquete(@PacientePaqueteId)` — con patrón idempotente `CREATE` + `IF OBJECT_ID`, y MUST NOT modificar tablas, FKs ni objetos existentes de la BD.

#### Scenario: Script idempotente

- GIVEN un SP `sp_nh_*` ya existente en la BD
- WHEN se ejecuta el script
- THEN el SP no se recrea ni altera (lo detecta `IF OBJECT_ID`)

### Requirement: Alcance solo lectura del change

El change MUST NOT implementar pagos ni altas (asociar paquete o guardar tratamiento del paquete), y MUST NOT modificar `Historial/Index`, `Cita/Index` ni `Consulta/Index`.

#### Scenario: Solo lectura verificada

- GIVEN el change implementado
- WHEN se inspeccionan las vistas y la capa de datos
- THEN no existen endpoints/SPs de escritura de paquetes ni se modificaron `Historial/Index`, `Cita/Index` o `Consulta/Index`
