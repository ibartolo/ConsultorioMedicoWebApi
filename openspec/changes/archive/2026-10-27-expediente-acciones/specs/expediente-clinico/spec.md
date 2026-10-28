# Delta para Expediente Clínico

> Change `expediente-acciones`. Delta sobre el capability `expediente-clinico` (spec viva en `openspec/changes/expediente-clinico-historial/specs/expediente-clinico/spec.md`, aún no archivada a `openspec/specs/`).

## ADDED Requirements

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

Al pulsar "Nuevo Tratamiento", el sistema SHALL abrir un modal de Cita con el paciente en contexto fijo (nombre en solo lectura + `input hidden` `PacienteID`) y los campos Tratamiento (opcional), Fecha y Hora (requerida) y Comentarios. Al guardar, el sistema MUST enviar el formulario al endpoint existente `CitaController.GuardarOActualizarCita` (MVC) → `api/Cita/Guardar` → `sp_nh_SaveCita`. Al guardar con éxito, el sistema SHALL mostrar un SweetAlert de éxito, cerrar el modal y recargar SOLO el grid `#tratamientosTable` (Tab 1). El sistema MUST NOT recargar el grid de consultas ni navegar fuera del expediente.

#### Scenario: Creación de cita (tratamiento) exitosa

- GIVEN el expediente abierto y el modal "Nuevo Tratamiento" con datos válidos
- WHEN el usuario guarda
- THEN se crea una Cita asociada al paciente en contexto vía `sp_nh_SaveCita`
- AND se muestra SweetAlert de éxito, se cierra el modal y se recarga solo `#tratamientosTable`

#### Scenario: Guardado fallido

- GIVEN datos inválidos o error del servidor
- WHEN el usuario guarda
- THEN se muestra SweetAlert de error y el modal permanece abierto

### Requirement: Modal "Nueva Consulta" (crea una Consulta con paciente fijo)

Al pulsar "Nueva Consulta", el sistema SHALL abrir un modal de Consulta con el paciente en contexto fijo (solo lectura + `input hidden` `PacienteID`) y los campos TipoConsulta (opcional), Tratamiento (opcional), Fecha y Hora (requerida), Descripción, Receta y Próxima Consulta. Al guardar, el sistema MUST enviar el formulario al endpoint existente `ConsultaController.GuardarOActualizarConsulta` (MVC) → `api/Consulta/Guardar` → `sp_nh_SaveConsulta`. Al guardar con éxito, el sistema SHALL mostrar un SweetAlert de éxito, cerrar el modal y recargar SOLO el grid `#consultasTable` (Tab 2). El sistema MUST NOT recargar el grid de tratamientos ni navegar fuera del expediente.

#### Scenario: Creación de consulta exitosa

- GIVEN el expediente abierto y el modal "Nueva Consulta" con datos válidos
- WHEN el usuario guarda
- THEN se crea una Consulta asociada al paciente en contexto vía `sp_nh_SaveConsulta`
- AND se muestra SweetAlert de éxito, se cierra el modal y se recarga solo `#consultasTable`

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
