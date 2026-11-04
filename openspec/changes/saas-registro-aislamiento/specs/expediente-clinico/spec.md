# Delta para Expediente Clínico

## ADDED Requirements

### Requirement: Aislamiento por consultorio del expediente

El expediente clínico SHALL filtrar por el tenant del usuario autenticado. Al abrir `/Historial/Expediente/{id}`, el sistema MUST validar que el paciente pertenezca al consultorio; si no, MUST devolver error controlado sin exponer datos. Los endpoints de tratamientos, consultas y paquetes del expediente MUST filtrar por el consultorio y validar pertenencia.

#### Scenario: Expediente de paciente propio

- GIVEN un paciente del consultorio del usuario
- WHEN se abre su expediente
- THEN se muestran sus datos, tratamientos, consultas y paquetes

#### Scenario: Expediente de paciente ajeno

- GIVEN un `{id}` de paciente de otro consultorio
- WHEN se intenta abrir el expediente
- THEN se muestra un error controlado y no se exponen datos

#### Scenario: Endpoints filtrados

- GIVEN un usuario del consultorio 1
- WHEN consulta tratamientos/consultas/paquetes de un paciente
- THEN solo obtiene datos del consultorio 1

## MODIFIED Requirements

### Requirement: Receta con membrete institucional, logo y firma

La receta SHALL tomar los datos de la institución del tenant actual desde `DatosInstitucion` (nombre, dirección, teléfonos, correo y `LogoRuta`) y los datos del médico (`MedicoNombre`, `MedicoCedula`, `MedicoEspecialidad`). El logo y los datos del médico SHALL configurarse en **Datos de la Institución** del consultorio actual. La receta MUST mostrar el paciente, la fecha, el tipo de consulta, el diagnóstico, la próxima consulta, el medicamento/indicaciones y la firma del médico.

(Previously: la receta usaba la institución única global; ahora resuelve `DatosInstitucion` del tenant.)

#### Scenario: Receta con logo y firma

- GIVEN una consulta y una institución del tenant con logo y médico configurados
- WHEN se abre la receta
- THEN se muestran el logo, el membrete, los datos del paciente y la firma del médico

#### Scenario: Receta con branding del tenant

- GIVEN dos consultorios con logos/nombres distintos
- WHEN un usuario del consultorio 2 abre la receta
- THEN la receta muestra el nombre y logo del consultorio 2
