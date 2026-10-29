# Especificación: Cita

## Purpose

Comportamiento del calendario de Citas (FullCalendar): pintar los eventos según su `Estado` (1 Pendiente, 2 Confirmada, 3 Cancelada) con colores y tooltip, y habilitar la baja lógica de una Cita (`Estatus=0`) mediante el SP `sp_nh_DeleteCita` y el endpoint `POST api/Cita/Eliminar/{id:int}` con botón "Eliminar" en el modal. Acceso a datos estrictamente ADO.NET vía SPs. Respuesta `ModelResponse<T>` (HTTP 200 siempre, mensajes en español).

## Requirements

### Requirement: Colores de eventos según estado

Los eventos del calendario de Citas MUST pintarse según el `Estado` del evento (`extendedProps.Estado`), aplicando color de fondo, borde y texto legible:

| Estado | Nombre | Color |
|--------|--------|-------|
| 1 | Pendiente | #FD7E14 (naranja) |
| 2 | Confirmada | #198754 (verde) |
| 3 | Cancelada | #DC3545 (rojo) |
| null/otro | Sin estado | #6C757D (gris) |

#### Scenario: Evento pendiente

- GIVEN una cita con `Estado=1`
- WHEN se renderiza el calendario de Citas
- THEN el evento se pinta en naranja #FD7E14

#### Scenario: Evento sin estado

- GIVEN una cita con `Estado` null o distinto de 1/2/3
- WHEN se renderiza el calendario
- THEN el evento se pinta en gris #6C757D

### Requirement: Tooltip con estado del evento

Al pasar el mouse sobre un evento, el sistema MUST mostrar un tooltip con el estado textual (Pendiente/Confirmada/Cancelada) y SHOULD incluir paciente/tratamiento cuando estén disponibles. Para `Estado` null, el tooltip SHALL decir "Sin estado".

#### Scenario: Tooltip de evento confirmado

- GIVEN un evento de una cita con `Estado=2`
- WHEN el usuario pasa el mouse sobre el evento
- THEN se muestra el tooltip "Confirmada" con paciente/tratamiento disponibles

#### Scenario: Tooltip de evento sin estado

- GIVEN un evento con `Estado` null
- WHEN el usuario pasa el mouse sobre el evento
- THEN se muestra el tooltip "Sin estado"

### Requirement: Baja lógica de Cita vía SP

El sistema SHALL exponer el SP `sp_nh_DeleteCita` (`@Id INT`) que ejecuta `UPDATE dbo.Cita SET Estatus = 0 WHERE Id = @Id`. El SP MUST crearse idempotente (CREATE + IF OBJECT_ID, sin DROP/ALTER). La eliminación MUST ser baja lógica (`Estatus=0`), nunca borrado físico.

#### Scenario: Baja lógica exitosa

- GIVEN una cita con `Id` existente
- WHEN se ejecuta `sp_nh_DeleteCita @Id`
- THEN `Estatus` queda en 0 y el registro persiste en la tabla

### Requirement: Endpoint de eliminación

El sistema SHALL exponer `POST api/Cita/Eliminar/{id:int}` (id por ruta) que recibe el `id` y SHALL retornar `ModelResponse<bool>` (HTTP 200, mensajes en español): `IsSuccess=true` con `Response=true` al completar la baja lógica; `IsSuccess=false` con `Message` de error ante fallo, id inexistente o id inválido (≤ 0). Flujo: Controller → Service → DbWrapper (`sp_nh_DeleteCita`).

#### Scenario: Eliminación exitosa

- GIVEN una cita activa con `Id>0`
- WHEN se llama `POST api/Cita/Eliminar/{id}`
- THEN responde 200 con `IsSuccess=true` y la cita queda con `Estatus=0`

#### Scenario: Error de eliminación

- GIVEN un fallo de BD o id inexistente
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=false` y `Message` en español

#### Scenario: Id inválido

- GIVEN un `id` no numérico o ≤ 0
- WHEN se llama el endpoint
- THEN responde 200 con `IsSuccess=false` y `Message` en español (no 400)

### Requirement: Acción MVC y botón Eliminar

El sistema SHALL exponer la acción MVC Data Access `EliminarCita(long id)` (POST → `JsonConvert.SerializeObject`) con su DAL (`HttpClientConnection.Cita.cs`) llamando a `api/Cita/Eliminar/{id}`. En el modal de Cita, el footer MUST mostrar el botón "Eliminar" (rojo) solo al editar una cita existente (`Id>0`); MUST NOT mostrarlo cuando `Id==0`. Al pulsarlo, el sistema SHALL pedir confirmación con SweetAlert ("¿Estás seguro que quieres eliminar?"); al confirmar SHALL hacer POST a la acción MVC; al éxito SHALL mostrar SweetAlert de éxito, cerrar el modal y ejecutar `calendar.refetchEvents()` (la cita desaparece porque el listado filtra `Estatus=1`).

#### Scenario: Botón visible al editar

- GIVEN el modal de Cita abierto para una cita existente (`Id>0`)
- WHEN se inspecciona el footer
- THEN se muestra el botón "Eliminar"

#### Scenario: Botón oculto al crear

- GIVEN el modal de Cita abierto para una cita nueva (`Id=0`)
- WHEN se inspecciona el footer
- THEN el botón "Eliminar" no se muestra

#### Scenario: Eliminación confirmada

- GIVEN una cita existente en edición
- WHEN el usuario pulsa "Eliminar" y confirma el SweetAlert
- THEN se hace POST a la acción MVC, se muestra éxito, se cierra el modal y `refetchEvents()` oculta la cita

#### Scenario: Eliminación cancelada

- GIVEN el SweetAlert de confirmación abierto
- WHEN el usuario cancela
- THEN no se hace POST y el modal permanece abierto

### Requirement: Intervalo y horario del calendario

El calendario de Citas SHALL mostrar intervalos y etiquetas cada **30 minutos** y SHALL limitar el horario visible de **08:00 a 20:00**. La vista de semana/día SHALL iniciar en las 08:00 y MUST ocultar la fila de "todo el día". La selección y el arrastre SHOULD ajustarse a bloques de 30 minutos.

#### Scenario: Calendario de semana con intervalo de 30 minutos

- GIVEN el calendario de Citas en vista de semana
- WHEN se renderiza
- THEN cada celda y etiqueta representa 30 minutos y el rango visible es 08:00–20:00

#### Scenario: Vista mensual

- GIVEN la vista mensual de Citas
- WHEN el usuario la selecciona
- THEN se mantiene la vista mensual; el rango horario aplica a las vistas de semana/día

## Notas (bugfix / operación — fuera de esta capability)

1. **Fix calendario de Consultas**: `Views/Consulta/Index.cshtml` (línea 119) corrige `initialView: 'timeGridWeek` → `initialView: 'timeGridWeek',` (coma final). Bugfix de sintaxis JS; no es requisito de esta capability.
2. **SP historial**: `sql/sp_nh_Historial.sql` (contiene `sp_nh_GetTratamientosPorPaciente`) se ejecuta contra la BD productiva para que el grid "Tratamientos Históricos" deje de estar vacío. Operación de BD, no requisito de esta capability.
