# Tasks: Acciones del Expediente (Nuevo Tratamiento / Nueva Consulta + fix de ancho)

> Change `expediente-acciones`. Solo MVC/Vista: `Expediente.cshtml` + `HistorialController.cs`. Sin endpoints/SPs/DTOs/servicios nuevos ni vistas a registrar (`Expediente.cshtml` ya está en el `.csproj`).

## Fase 1: Vista — fix ancho (`Views/Historial/Expediente.cshtml`)

- [x] 1.1 Quitar `responsive: true` de `#tratamientosTable` (línea ~115) y `#consultasTable` (línea ~132); agregar `style="width:100%"` a ambos `<table>` (líneas ~82 y ~94).
- [x] 1.2 Declarar `var tratamientosTable, consultasTable;` a nivel de página (fuera de `$(document).ready`), asignar sin `var` dentro de `ready`, y agregar `$.fn.dataTable.tables({ visible: true, api: true }).columns.adjust();` al final del `ready` (mantener el handler `shown.bs.tab` existente).

## Fase 2: Vista — header botones (`Views/Historial/Expediente.cshtml`)

- [x] 2.1 Sustituir el `<a ...>Volver a Pacientes</a>` (líneas ~7-9) por `<span class="float-end">` con: botón "Nuevo Tratamiento" (`onclick="nuevaCitaExpediente()"`), botón "Nueva Consulta" (`onclick="nuevaConsultaExpediente()"`) y el link "Volver a Pacientes" (todos `btn-sm`, iconos `fa-plus`/`fa-arrow-left`).

## Fase 3: Vista — modal Cita (`Views/Historial/Expediente.cshtml`)

- [x] 3.1 Agregar modal `#modalCitaExpediente` con form `#citaFormExpediente` (`autocomplete="off" novalidate`): hidden `Id=0`, `Estatus=true`, `PacienteID=@Model.Id`; nombre del paciente solo lectura; select `TratamientoID` (`ViewBag.Tratamientos`); `FechaCita` datetime-local; select `Estado` (1/2/3); `Comentarios`. Campos por `name` SIN `id` (evitar colisión con el modal Consulta).
- [x] 3.2 Agregar JS: `$('#citaFormExpediente').validate(...)` con `FechaCita: { required: true }` (mismas opciones `errorElement/errorClass/highlight/unhighlight`); `nuevaCitaExpediente()` = reset + `modal('show')`; `guardarCitaExpediente()` = valid → `$.ajax POST @Url.Action("GuardarOActualizarCita","Cita")` con `serialize()` → éxito `Swal` timer 1500 → hide → `tratamientosTable.ajax.reload()`.

## Fase 4: Vista — modal Consulta (`Views/Historial/Expediente.cshtml`)

- [x] 4.1 Agregar modal `#modalConsultaExpediente` con form `#consultaFormExpediente`: hidden `Id`/`Estatus`/`PacienteID`; nombre solo lectura; select `TipoConsultaID` (`ViewBag.TiposConsulta`); select `TratamientoID`; `FechaConsulta` + `FechaProximaConsulta` datetime-local; `Descripcion`; textarea `DescripcionReceta`; textarea `Comentarios`. Sin `#btnReceta`. Campos por `name`.
- [x] 4.2 Agregar JS: `$('#consultaFormExpediente').validate(...)` con `FechaConsulta: { required: true }`; `nuevaConsultaExpediente()`/`guardarConsultaExpediente()` → `POST @Url.Action("GuardarOActualizarConsulta","Consulta")` → éxito Swal → hide → `consultasTable.ajax.reload()`.

## Fase 5: Controller — ViewBags (`Controllers/HistorialController.cs`)

- [x] 5.1 Agregar campo `private readonly TipoConsultaService _tipoConsultaService;` e instanciarlo en el constructor (`new TipoConsultaService(httpClientConnection)`). (`_citaService` ya existe.)
- [x] 5.2 En `Expediente(long id)`, antes de `return View(paciente)`: `ViewBag.Tratamientos` vía `await _citaService.ConsultarTratamientos()` y `ViewBag.TiposConsulta` vía `await _tipoConsultaService.ConsultarTiposConsulta()`, con guard `IsSuccess` → `new List<...>()`.

## Fase 6: Verificación

- [x] 6.1 `MSBuild.exe DESI.ConsultorioMedico.sln /p:Configuration=Debug` → exit 0.
- [x] 6.2 `aspnet_compiler` sobre `ConsultorioMedicoMVC` → exit 0.
- [ ] 6.3 Manual: grids a 100% al cargar y al cambiar de tab; ambos modales abren con paciente fijo, validan fecha, guardan con Swal y recargan solo su grid; `Cita/Index`, `Consulta/Index` e `Historial/Index` intactos.
