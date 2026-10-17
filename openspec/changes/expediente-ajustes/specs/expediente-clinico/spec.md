# Delta para Expediente Clínico

> Change `expediente-ajustes`. Delta sobre el capability `expediente-clinico` (spec viva en `openspec/changes/expediente-clinico-historial/specs/expediente-clinico/spec.md` y `openspec/changes/expediente-acciones/specs/expediente-clinico/spec.md`, aún no archivada a `openspec/specs/`).

## MODIFIED Requirements

### Requirement: Grid de tratamientos históricos (Tab 1)

El sistema SHALL mostrar un grid de tratamientos derivados SOLO de Cita, obtenidos del endpoint `api/Historial/TratamientosPorPaciente/{pacienteId}`, ordenados por fecha descendente. Cada fila SHALL exponer fecha, nombre del tratamiento, origen y comentarios. El sistema MUST NOT incluir tratamientos de Consulta en este grid; el campo Origen SHALL mostrar el valor constante "Cita".

(Previously: el grid mezclaba Cita y Consulta vía UNION con Origen "Cita"/"Consulta".)

#### Scenario: Tratamientos listados (solo citas)

- GIVEN un paciente con citas asociadas a tratamientos
- WHEN se carga el grid de tratamientos
- THEN se listan solo las citas, ordenadas por fecha descendente, con Origen "Cita"

#### Scenario: Paciente con consultas y sin citas

- GIVEN un paciente con 1 consulta y 0 citas
- WHEN se carga el grid
- THEN el grid queda vacío sin error

### Requirement: Endpoint de tratamientos por paciente

El sistema SHALL exponer `api/Historial/TratamientosPorPaciente/{pacienteId}` (solo lectura) que retorne `ModelResponse<T>` (HTTP 200, español, camelCase). Sus datos MUST derivarse del SP `sp_nh_GetTratamientosPorPaciente`, que SHALL devolver SOLO citas y MUST NOT hacer UNION con Consulta. El DTO `TratamientoHistoricoDTO` SHALL conservar `Id, Fecha, TratamientoNombre, Origen, Comentarios` con `Origen` constante "Cita" (contrato preservado). El SP ya existe en BD y se actualiza con patrón idempotente DROP+CREATE (`IF OBJECT_ID(...) IS NOT NULL DROP PROCEDURE ...; CREATE PROCEDURE ...`).

(Previously: el SP hacía UNION de Cita y Consulta.)

#### Scenario: Respuesta exitosa

- GIVEN un `pacienteId` válido
- WHEN se llama `GET api/Historial/TratamientosPorPaciente/{pacienteId}`
- THEN responde 200 con `IsSuccess=true` y la lista de citas en `Response`

#### Scenario: Paciente con consultas y sin citas

- GIVEN un paciente con 1 consulta y 0 citas
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=true` y lista vacía

#### Scenario: Id inválido

- GIVEN un `pacienteId` no numérico o ≤ 0
- WHEN se llama el endpoint
- THEN responde 400 por validación

### Requirement: Modal "Nuevo Tratamiento" (crea una Cita con paciente fijo)

Al pulsar "Nuevo Tratamiento", el sistema SHALL abrir un modal de Cita con el paciente en contexto fijo (solo lectura + `input hidden` `PacienteID`) y los campos Tratamiento (opcional), Fecha y Hora (requerida) y Comentarios, que SHALL ser un `<textarea>` amplio (ancho completo, varias filas). Al guardar, el sistema MUST enviar el formulario al endpoint existente `CitaController.GuardarOActualizarCita` (MVC) → `api/Cita/Guardar` → `sp_nh_SaveCita`. Al guardar con éxito, SHALL mostrar SweetAlert de éxito, cerrar el modal y recargar SOLO `#tratamientosTable`; MUST NOT recargar el grid de consultas ni navegar fuera del expediente.

(Previously: Comentarios era un `input` de texto de una línea.)

#### Scenario: Campo Comentarios amplio

- GIVEN el modal "Nuevo Tratamiento" abierto
- WHEN se inspecciona el campo Comentarios
- THEN es un `<textarea>` de ancho completo con varias filas

#### Scenario: Creación exitosa

- GIVEN el modal con datos válidos
- WHEN el usuario guarda
- THEN se crea una Cita vía `sp_nh_SaveCita`, se muestra SweetAlert de éxito, se cierra el modal y se recarga solo `#tratamientosTable`

#### Scenario: Guardado fallido

- GIVEN datos inválidos o error del servidor
- WHEN el usuario guarda
- THEN se muestra SweetAlert de error y el modal permanece abierto

### Requirement: Modal "Nueva Consulta" (crea una Consulta con paciente fijo)

Al pulsar "Nueva Consulta", el sistema SHALL abrir un modal de Consulta con el paciente en contexto fijo (solo lectura + `input hidden` `PacienteID`) y los campos TipoConsulta (opcional), Tratamiento (opcional), Fecha y Hora (requerida), Comentarios (que SHALL ser un `<textarea>` amplio) y Próxima Consulta. El sistema MUST NOT incluir los campos Descripción ni Receta en este modal. Al guardar, el sistema MUST enviar el formulario al endpoint existente `ConsultaController.GuardarOActualizarConsulta` (MVC) → `api/Consulta/Guardar` → `sp_nh_SaveConsulta`; `Descripcion`/`DescripcionReceta` son nullable y, al no enviarse, quedan null, por lo que el guardado MUST seguir funcionando. Al guardar con éxito, SHALL mostrar SweetAlert de éxito, cerrar el modal y recargar SOLO `#consultasTable`; MUST NOT recargar el grid de tratamientos.

(Previously: el modal incluía Descripción y Receta y Comentarios era de 2 filas.)

#### Scenario: Modal sin Descripción ni Receta

- GIVEN el modal "Nueva Consulta" abierto
- WHEN se inspeccionan los campos
- THEN no existen los campos Descripción ni Receta y Comentarios es un `<textarea>` amplio

#### Scenario: Creación exitosa

- GIVEN el modal con datos válidos
- WHEN el usuario guarda
- THEN se crea una Consulta vía `sp_nh_SaveConsulta`, se muestra SweetAlert de éxito, se cierra el modal y se recarga solo `#consultasTable`

#### Scenario: Guardado fallido

- GIVEN datos inválidos o error del servidor
- WHEN el usuario guarda
- THEN se muestra SweetAlert de error y el modal permanece abierto

## ADDED Requirements

### Requirement: Formato de fecha del grid de tratamientos

La columna Fecha del grid `#tratamientosTable` MUST mostrar las fechas en formato `dd/MM/yyyy hh:mm am/pm` (reloj de 12 horas con sufijo am/pm), mediante render JS manual (sin moment.js). Una fecha nula MUST mostrarse como celda vacía.

#### Scenario: Fecha formateada

- GIVEN una cita con fecha
- WHEN se renderiza el grid
- THEN la columna Fecha muestra `dd/MM/yyyy hh:mm am/pm`

#### Scenario: Fecha nula

- GIVEN una cita con fecha nula
- WHEN se renderiza el grid
- THEN la celda Fecha queda vacía

### Requirement: Aislamiento del change

El change MUST NOT modificar `Views/Consulta/Index.cshtml`, `Views/Cita/Index.cshtml` ni `Historial/Index`, y MUST NOT crear endpoints, SPs ni DTOs nuevos. La pestaña Paquetes SHALL permanecer fuera de alcance.

#### Scenario: Vistas no afectadas

- GIVEN el change implementado
- WHEN se inspeccionan las vistas de Cita y Consulta y `Historial/Index`
- THEN permanecen intactas y el modal de `Consulta/Index.cshtml` conserva Descripción y Receta
