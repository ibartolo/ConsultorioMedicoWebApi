# Especificación: Expediente Clínico

## Purpose

Vista consolidada de **solo lectura** del expediente de un paciente, accesible desde el grid de Pacientes. Agrupa en pestañas Bootstrap 5 los datos del paciente, el historial de tratamientos (derivado de Cita+Consulta) y el historial de consultas con drill-down a la receta. Las pestañas "Archivos" y "Paquetes" quedan ocultas (diferidas). El cambio es aditivo; `Historial/Index` permanece intacto.

## Requirements

### Requirement: Acceso al expediente desde el grid de Pacientes

El sistema MUST mostrar un botón "Expediente Clínico" por fila en `Views/Paciente/Index.cshtml`, junto al botón de edición, cuyo enlace SHALL apuntar a `/Historial/Expediente/{id}`.

#### Scenario: Apertura del expediente

- GIVEN un paciente con `Id` en el grid de Pacientes
- WHEN el usuario pulsa "Expediente Clínico" en su fila
- THEN se navega a `/Historial/Expediente/{id}` con el paciente en contexto

### Requirement: Vista de expediente con pestañas

La vista `Views/Historial/Expediente.cshtml` SHALL mostrar 4 pestañas Bootstrap 5 (`nav-tabs`): "Información del paciente", "Historial de consultas", "Archivos" y "Paquetes". Las pestañas "Archivos" y "Paquetes" MUST permanecer ocultas (`d-none`) en este change.

#### Scenario: Navegación entre pestañas visibles

- GIVEN el expediente abierto
- WHEN el usuario selecciona "Información del paciente" o "Historial de consultas"
- THEN se muestra el contenido de la pestaña seleccionada

#### Scenario: Pestañas diferidas ocultas

- GIVEN el expediente abierto
- WHEN se inspeccionan las pestañas
- THEN "Archivos" y "Paquetes" no son visibles ni interactivas

### Requirement: Panel de información del paciente

El sistema SHALL mostrar un panel con los datos del paciente obtenidos de `api/Paciente/{id}`. El nombre completo SHALL componerse en el lado MVC a partir de `Nombre`, `ApellidoPaterno` y `ApellidoMaterno` (el endpoint no devuelve `NombreCompleto`).

#### Scenario: Panel con datos del paciente

- GIVEN un paciente con `Id` válido
- WHEN se renderiza la Tab 1
- THEN el panel muestra los datos del paciente con el nombre completo compuesto

### Requirement: Grid de tratamientos históricos (Tab 1)

El sistema SHALL mostrar un grid de tratamientos derivados de Cita y Consulta, obtenidos del endpoint `api/Historial/TratamientosPorPaciente/{pacienteId}`, ordenados por fecha descendente. Cada fila SHALL exponer: fecha, nombre del tratamiento, origen (Cita/Consulta) y comentarios.

#### Scenario: Tratamientos listados

- GIVEN un paciente con citas y/o consultas asociadas a tratamientos
- WHEN se carga el grid de tratamientos
- THEN se listan los tratamientos de Cita y Consulta, ordenados por fecha descendente, con su origen

#### Scenario: Paciente sin tratamientos

- GIVEN un paciente sin citas ni consultas con tratamiento
- WHEN se carga el grid
- THEN el grid queda vacío sin error

### Requirement: Historial de consultas (Tab 2) con drill-down

El sistema SHALL mostrar el grid de consultas del paciente reutilizando el endpoint existente `api/Consulta/PorPaciente/{pacienteId}`. Al hacer doble clic en una fila, el sistema SHALL abrir `Consulta/Receta/{id}` en una pestaña nueva.

#### Scenario: Drill-down a la receta

- GIVEN una consulta listada en la Tab 2
- WHEN el usuario hace doble clic en la fila
- THEN se abre `Consulta/Receta/{id}` en una pestaña nueva del navegador

### Requirement: Endpoint de tratamientos por paciente

El sistema SHALL exponer un endpoint de solo lectura `api/Historial/TratamientosPorPaciente/{pacienteId}` que retorne `ModelResponse<T>` (HTTP 200, mensajes en español, camelCase). Sus datos MUST derivarse del SP nuevo `sp_nh_GetTratamientosPorPaciente` (UNION de Cita y Consulta con JOIN a Tratamiento). El DTO de respuesta `TratamientoHistoricoDTO` SHALL contener `Id, Fecha, TratamientoNombre, Origen, Comentarios`.

#### Scenario: Respuesta exitosa

- GIVEN un `pacienteId` válido
- WHEN se llama `GET api/Historial/TratamientosPorPaciente/{pacienteId}`
- THEN responde 200 con `IsSuccess=true` y la lista de tratamientos en `Response`

#### Scenario: Paciente sin tratamientos

- GIVEN un `pacienteId` sin registros asociados
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=true` y lista vacía

#### Scenario: Id inválido

- GIVEN un `pacienteId` no numérico o ≤ 0
- WHEN se llama el endpoint
- THEN responde 400 por validación

## Nota (pregunta abierta → change 2)

¿El grid de tratamientos debe incluir también tratamientos provenientes de **paquetes**? Ello exige la capa `PacientePaquete`/`TratamientoPaquete` (change 2 `paquetes-del-paciente`). En este change se deriva solo de Cita+Consulta; la inclusión de tratamientos de paquetes queda pendiente de confirmar.
