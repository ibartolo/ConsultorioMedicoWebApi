# Delta para Expediente Clínico

> Change `paquetes-lectura`. Delta sobre el capability `expediente-clinico` (spec viva en `openspec/changes/expediente-clinico-historial/specs/expediente-clinico/spec.md` y deltas `expediente-acciones` y `expediente-ajustes`, aún no archivada a `openspec/specs/`).

## MODIFIED Requirements

### Requirement: Vista de expediente con pestañas

La vista `Views/Historial/Expediente.cshtml` SHALL mostrar 4 pestañas Bootstrap 5 (`nav-tabs`): "Información del paciente", "Historial de consultas", "Archivos" y "Paquetes". La pestaña "Paquetes" MUST estar visible (sin `d-none`) y mostrar dos grids de solo lectura: los paquetes adquiridos por el paciente y, al seleccionar uno, sus citas-tratamientos. La pestaña "Archivos" MUST permanecer oculta (`d-none`).

(Previously: las pestañas "Archivos" y "Paquetes" permanecían ocultas (`d-none`) y sin contenido.)

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
