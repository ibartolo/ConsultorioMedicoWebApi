# Especificación: Expediente Clínico

## Purpose

Vista consolidada de **solo lectura** del expediente de un paciente, accesible desde el grid de Pacientes. Agrupa en pestañas Bootstrap 5 los datos del paciente, el historial de tratamientos (derivado de Cita) y el historial de consultas con drill-down a la receta. La pestaña "Archivos" queda oculta (diferida); la pestaña "Paquetes" muestra los paquetes adquiridos y sus citas-tratamientos en solo lectura. El cambio es aditivo; `Historial/Index` permanece intacto.

## Requirements

### Requirement: Acceso al expediente desde el grid de Pacientes

El sistema MUST mostrar un botón "Expediente Clínico" por fila en `Views/Paciente/Index.cshtml`, junto al botón de edición, cuyo enlace SHALL apuntar a `/Historial/Expediente/{id}`.

#### Scenario: Apertura del expediente

- GIVEN un paciente con `Id` en el grid de Pacientes
- WHEN el usuario pulsa "Expediente Clínico" en su fila
- THEN se navega a `/Historial/Expediente/{id}` con el paciente en contexto

### Requirement: Vista de expediente con pestañas

La vista `Views/Historial/Expediente.cshtml` SHALL mostrar 4 pestañas Bootstrap 5 (`nav-tabs`): "Información del paciente", "Historial de consultas", "Archivos" y "Paquetes". La pestaña "Paquetes" MUST estar visible (sin `d-none`) y mostrar dos grids de solo lectura: los paquetes adquiridos por el paciente y, al seleccionar uno, sus citas-tratamientos. La pestaña "Archivos" MUST permanecer oculta (`d-none`).

#### Scenario: Navegación entre pestañas visibles

- GIVEN el expediente abierto
- WHEN el usuario selecciona "Información del paciente", "Historial de consultas" o "Paquetes"
- THEN se muestra el contenido de la pestaña seleccionada

#### Scenario: Pestaña Archivos diferida oculta

- GIVEN el expediente abierto
- WHEN se inspeccionan las pestañas
- THEN "Archivos" no es visible ni interactiva

#### Scenario: Pestaña Paquetes visible con grids

- GIVEN el expediente abierto
- WHEN el usuario selecciona la pestaña "Paquetes"
- THEN la pestaña es visible y muestra dos grids de solo lectura (paquetes y citas-tratamientos)

### Requirement: Panel de información del paciente

El sistema SHALL mostrar un panel con los datos del paciente obtenidos de `api/Paciente/{id}`. El nombre completo SHALL componerse en el lado MVC a partir de `Nombre`, `ApellidoPaterno` y `ApellidoMaterno` (el endpoint no devuelve `NombreCompleto`).

#### Scenario: Panel con datos del paciente

- GIVEN un paciente con `Id` válido
- WHEN se renderiza la Tab 1
- THEN el panel muestra los datos del paciente con el nombre completo compuesto

### Requirement: Grid de tratamientos históricos (Tab 1)

El sistema SHALL mostrar un grid de tratamientos derivados SOLO de Cita, obtenidos del endpoint `api/Historial/TratamientosPorPaciente/{pacienteId}`, ordenados por fecha descendente. Cada fila SHALL exponer fecha, nombre del tratamiento, origen y comentarios. El sistema MUST NOT incluir tratamientos de Consulta en este grid; el campo Origen SHALL mostrar el valor constante "Cita".

#### Scenario: Tratamientos listados (solo citas)

- GIVEN un paciente con citas asociadas a tratamientos
- WHEN se carga el grid de tratamientos
- THEN se listan solo las citas, ordenadas por fecha descendente, con Origen "Cita"

#### Scenario: Paciente con consultas y sin citas

- GIVEN un paciente con 1 consulta y 0 citas
- WHEN se carga el grid
- THEN el grid queda vacío sin error

### Requirement: Historial de consultas (Tab 2) con drill-down

El sistema SHALL mostrar el grid de consultas del paciente reutilizando el endpoint existente `api/Consulta/PorPaciente/{pacienteId}`. Al hacer doble clic en una fila, el sistema SHALL abrir `Consulta/Receta/{id}` en una pestaña nueva.

#### Scenario: Drill-down a la receta

- GIVEN una consulta listada en la Tab 2
- WHEN el usuario hace doble clic en la fila
- THEN se abre `Consulta/Receta/{id}` en una pestaña nueva del navegador

### Requirement: Endpoint de tratamientos por paciente

El sistema SHALL exponer `api/Historial/TratamientosPorPaciente/{pacienteId}` (solo lectura) que retorne `ModelResponse<T>` (HTTP 200, mensajes en español, camelCase). Sus datos MUST derivarse del SP `sp_nh_GetTratamientosPorPaciente`, que SHALL devolver SOLO citas y MUST NOT hacer UNION con Consulta. El DTO `TratamientoHistoricoDTO` SHALL conservar `Id, Fecha, TratamientoNombre, Origen, Comentarios` con `Origen` constante "Cita" (contrato preservado). El SP ya existe en BD y se actualiza con patrón idempotente DROP+CREATE (`IF OBJECT_ID(...) IS NOT NULL DROP PROCEDURE ...; CREATE PROCEDURE ...`).

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

### Requirement: Ancho 100% de las DataTables del expediente

Las dos DataTables del expediente (`#tratamientosTable` en Tab 1 y `#consultasTable` en Tab 2) MUST ocupar el 100% del ancho disponible en todo momento — al cargar y al cambiar de pestaña — alineadas con el contenido del header. El sistema MUST NOT aplicar `responsive: true` a dichas tablas y SHALL reajustar las columnas al cargar y en cada evento `shown.bs.tab`.

#### Scenario: Grid a ancho completo al cargar

- GIVEN el expediente abierto con la Tab 1 visible
- WHEN se renderiza la DataTable de tratamientos
- THEN `#tratamientosTable` ocupa el 100% del ancho del contenedor y se alinea con el header

#### Scenario: Grid a ancho completo al cambiar de pestaña

- GIVEN el expediente abierto
- WHEN el usuario selecciona la Tab 2 "Historial de consultas"
- THEN `#consultasTable` ocupa el 100% del ancho sin encogerse

### Requirement: Botones de acción en el header del expediente

El header del expediente SHALL mostrar, junto a "Volver a Pacientes" (arriba), los botones "Nuevo Tratamiento" y "Nueva Consulta".

#### Scenario: Header con botones de acción

- GIVEN el expediente abierto
- WHEN se inspecciona el header
- THEN se muestran "Nuevo Tratamiento", "Nueva Consulta" y "Volver a Pacientes" alineados arriba

### Requirement: Modal "Nuevo Tratamiento" (crea una Cita con paciente fijo)

Al pulsar "Nuevo Tratamiento", el sistema SHALL abrir un modal de Cita con el paciente en contexto fijo (solo lectura + `input hidden` `PacienteID`) y los campos Tratamiento (opcional), Fecha y Hora (requerida) y Comentarios, que SHALL ser un `<textarea>` amplio (ancho completo, varias filas). Al guardar, el sistema MUST enviar el formulario al endpoint existente `CitaController.GuardarOActualizarCita` (MVC) → `api/Cita/Guardar` → `sp_nh_SaveCita`. Al guardar con éxito, SHALL mostrar SweetAlert de éxito, cerrar el modal y recargar SOLO `#tratamientosTable`; MUST NOT recargar el grid de consultas ni navegar fuera del expediente.

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

### Requirement: Validación de los modales del expediente

Los modales del expediente SHALL aplicar las mismas reglas de validación que los modales existentes de Cita y Consulta: `PacienteID` y fecha son requeridos; Tratamiento y TipoConsulta son opcionales. El sistema MUST NOT validar choque/colisión de horarios al guardar (la WinForms original no lo hacía; evidencia §5).

#### Scenario: Fecha requerida

- GIVEN el modal "Nuevo Tratamiento" o "Nueva Consulta" abierto sin fecha
- WHEN el usuario intenta guardar
- THEN la validación impide el envío y muestra el mensaje de campo requerido

### Requirement: Reutilización de endpoints y contratos existentes

El change MUST NOT crear endpoints, SPs, DTOs ni entidades nuevos. Los modales SHALL reutilizar los endpoints MVC/WebApi y SPs existentes (`sp_nh_SaveCita`, `sp_nh_SaveConsulta`). Los dropdowns Tratamiento/TipoConsulta SHALL poblarse desde los datos cargados por `HistorialController.Expediente(id)`.

#### Scenario: Sin artefactos nuevos

- GIVEN el change implementado
- WHEN se inspecciona la capa de datos y la API
- THEN no se han añadido endpoints, SPs ni DTOs nuevos

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

## Nota (pregunta abierta → change 2)

¿El grid de tratamientos debe incluir también tratamientos provenientes de **paquetes**? Ello exige la capa `PacientePaquete`/`TratamientoPaquete` (change 2 `paquetes-del-paciente`). En este change se deriva solo de Cita; la inclusión de tratamientos de paquetes queda pendiente de confirmar.
