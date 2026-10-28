# Design: Acciones del Expediente (Nuevo Tratamiento / Nueva Consulta + fix de ancho)

## Contexto

`Expediente.cshtml` ya existe (change `expediente-clinico-historial`): tabs Bootstrap 5 con Tab 1 (`tab-info`, visible) que aloja `#tratamientosTable` (citas+consultas derivadas) y Tab 2 (`tab-consultas`, `tab-pane fade` sin `active`, oculta) con `#consultasTable`; ambas DataTables con `responsive:true` y un handler `shown.bs.tab` que llama `columns.adjust()`. Este change agrega: (1) ancho 100% siempre de ambos grids y (2) dos modales autocontenidos (Cita con paciente fijo y Consulta con paciente fijo) disparados desde botones del header, reutilizando los endpoints POST existentes. Cambio aditivo, solo MVC/Vista, sin endpoints/SPs/DTOs/servicios nuevos.

## Decisiones de Arquitectura

| # | Decisión | Rationale | Alternativa rechazada |
|---|----------|-----------|----------------------|
| D1 | **Opción (a)**: modales copiados dentro de `Expediente.cshtml`, paciente FIJO | Mantiene el UX modal sin navegar fuera del expediente; consistente con el patrón de vistas autocontenidas del proyecto; sin refactor a vistas existentes | (b) parametrizar `Cita/Index`/`Consulta/Index` (rompe modal + riesgo de regresión); (c) partials (sin precedente en el proyecto) |
| D2 | Naming sin colisión: `#modalCitaExpediente`/`#citaFormExpediente` y `#modalConsultaExpediente`/`#consultaFormExpediente`; campos por `name` (sin `id` duplicados) | Dos formularios en la MISMA página: `name` puede repetirse entre `<form>` distintos (validate es per-form; solo se serializa un form por POST), pero `id` duplicado es HTML inválido y rompe `$('#x')` | `id` únicos por campo (innecesario, no hay que poblar valores: son altas) |
| D3 | Paciente fijo: `<input type="hidden" name="PacienteID" value="@Model.Id">` + nombre completo en solo lectura (etiqueta, no campo). Se OMITE regla `PacienteID required` | jQuery Validate ignora `hidden` por defecto; el valor siempre existe por `@Model.Id` | `<select disabled>` (no serializa) / `ignore: []` + required (complejidad innecesaria) |
| D4 | Reutilizar POST MVC existentes (`CitaController.GuardarOActualizarCita`, `ConsultaController.GuardarOActualizarConsulta`) con `$('#form').serialize()` (form-data); callback éxito: `Swal` timer 1500 → cerrar modal → `tratamientosTable.ajax.reload()` / `consultasTable.ajax.reload()` | Cero artefactos nuevos; mismo contrato que hoy; recarga SOLO el grid afectado | Endpoint AJAX nuevo (viola spec "Sin artefactos nuevos") |
| D5 | `HistorialController.Expediente` carga `ViewBag.Tratamientos` (vía `_citaService.ConsultarTratamientos()`) y `ViewBag.TiposConsulta` (vía nuevo campo `_tipoConsultaService`) | `_citaService` ya expone `ConsultarTratamientos()` (mismo endpoint que `TratamientoService`); espeja `CitaController.Index`. `_tipoConsultaService` es el único campo nuevo a instanciar | `TratamientoService` nuevo (redundante: mismo `ObtenerTratamientos()`) |
| D6 | Fix ancho: quitar `responsive:true` de ambas tablas; forzar `style="width:100%"` en los `<table>`; conservar `columns.adjust()` en `shown.bs.tab`; init **eager** de ambas en `ready` | `responsive` es la causa raíz del encogimiento (DataTables 1.13.6 + Bootstrap 5); `.tab-pane fade` sin `active` = `display:none` → la Tab 2 mide 0 y `columns.adjust()` la re-mide al mostrarse | init **lazy** de `consultasTable` en el primer `shown.bs.tab` (más complejo, rompe el patrón y el binding `dblclick`) |
| D7 | Duplicación de markup/JS de modales **aceptada** (riesgo de deriva) | Menor costo hoy vs. refactor; consistente con vistas autocontenidas | Extraer partial (se anota como mitigación futura si el markup crece) |

## Diagramas de Secuencia

**Flujo 1 — "Nuevo Tratamiento" (crea Cita con paciente fijo):**

```
Header click "Nuevo Tratamiento"
  └─> nuevaCitaExpediente(): reset #citaFormExpediente (PacienteID ya fijo = @Model.Id)
        └─> $('#modalCitaExpediente').modal('show')   [FechaCita/Tratamiento/Estado/Comentarios]
  └─> guardarCitaExpediente(): $('#citaFormExpediente').valid()  [FechaCita required]
        └─> $.ajax POST Cita/GuardarOActualizarCita  (data: $('#citaFormExpediente').serialize())
              └─> CitaController.GuardarOActualizarCita(Cita) → CitaService → api/Cita/Guardar → sp_nh_SaveCita
        <── ModelResponse<Cita>
        └─> IsSuccess: Swal.success(timer 1500) → $('#modalCitaExpediente').modal('hide')
                       → tratamientosTable.ajax.reload()   // SOLO Tab 1
        └─> !IsSuccess/error: Swal.error → modal permanece abierto
```

**Flujo 2 — "Nueva Consulta" (crea Consulta con paciente fijo):**

```
Header click "Nueva Consulta"
  └─> nuevaConsultaExpediente(): reset #consultaFormExpediente (PacienteID fijo = @Model.Id)
        └─> $('#modalConsultaExpediente').modal('show')
  └─> guardarConsultaExpediente(): $('#consultaFormExpediente').valid()  [FechaConsulta required]
        └─> $.ajax POST Consulta/GuardarOActualizarConsulta (serialize)
              └─> ConsultaController.GuardarOActualizarConsulta(Consulta) → ConsultaService → api/Consulta/Guardar → sp_nh_SaveConsulta
        <── ModelResponse<Consulta>
        └─> IsSuccess: Swal.success → hide → consultasTable.ajax.reload()   // SOLO Tab 2
        └─> !IsSuccess/error: Swal.error → modal abierto
```

**Flujo 3 — Fix de ancho en render:**

```
$(document).ready
  ├─> tratamientosTable init (tab-info VISIBLE)  → mide ancho 100% (sin responsive)
  └─> consultasTable init (tab-consultas display:none) → mide 0 (se corrige al mostrarse)
usuario click Tab 2
  └─> shown.bs.tab → $.fn.dataTable.tables({visible:true, api:true}).columns.adjust()
        └─> consultasTable recalcula ancho 100%
```

## Detalle por Capa (solo MVC/Vista)

### `Views/Historial/Expediente.cshtml` (modificar)

**Header** — agregar botones junto a "Volver a Pacientes" (contenedor `float-end`):

```html
<span class="float-end">
    <button type="button" class="btn btn-primary btn-sm me-1" onclick="nuevaCitaExpediente()">
        <i class="fas fa-plus me-1"></i>Nuevo Tratamiento</button>
    <button type="button" class="btn btn-primary btn-sm me-1" onclick="nuevaConsultaExpediente()">
        <i class="fas fa-plus me-1"></i>Nueva Consulta</button>
    <a href="@Url.Action("Index","Paciente")" class="btn btn-secondary btn-sm">
        <i class="fas fa-arrow-left me-1"></i>Volver a Pacientes</a>
</span>
```

**Modal Cita** (`#modalCitaExpediente`, form `#citaFormExpediente`) — copia de `#citaForm` SIN combo de pacientes; campos por `name` (sin `id` duplicados): `hidden Id=0`, `hidden Estatus=true`, `hidden PacienteID=@Model.Id`, `select TratamientoID` (opciones de `ViewBag.Tratamientos`), `input datetime-local FechaCita`, `select Estado` (1/2/3), `input Comentarios`. Nombre del paciente en solo lectura: `<div class="form-control-plaintext fw-bold">@Model.Nombre @Model.ApellidoPaterno @Model.ApellidoMaterno</div>`.

**Modal Consulta** (`#modalConsultaExpediente`, form `#consultaFormExpediente`) — copia de `#consultaForm` SIN combo ni `#btnReceta` (siempre alta): `hidden Id`, `hidden Estatus`, `hidden PacienteID`, `select TipoConsultaID` (`ViewBag.TiposConsulta`), `select TratamientoID` (`ViewBag.Tratamientos`), `input FechaConsulta`, `input FechaProximaConsulta`, `input Descripcion`, `textarea DescripcionReceta`, `textarea Comentarios`.

**JS** (`@section scripts`):
- Declarar a nivel de página (fuera de `ready`, como `calendar` en Cita/Index): `var tratamientosTable, consultasTable;` — los callbacks `guardar*` globales necesitan acceso.
- En `ready`: asignar ambas DataTables **sin** `responsive:true`, con `style="width:100%"` en los `<table>`.
- `$('#citaFormExpediente').validate({ rules: { FechaCita: { required: true } }, ... })` y `$('#consultaFormExpediente').validate({ rules: { FechaConsulta: { required: true } }, ... })` — mismas opciones `errorElement/errorClass/highlight/unhighlight` que Cita/Consulta.
- `nuevaCitaExpediente()` = reset + `modal('show')`; `guardarCitaExpediente()` = valid → `$.ajax` POST `@Url.Action("GuardarOActualizarCita","Cita")` con `serialize()` → éxito: `Swal` timer 1500 → hide → `tratamientosTable.ajax.reload()`. Análogo `nuevaConsultaExpediente()`/`guardarConsultaExpediente()` → `consultasTable.ajax.reload()`.
- Conservar `shown.bs.tab` → `columns.adjust()` y el `dblclick` de `consultasTable`.

### `Controllers/HistorialController.cs` (modificar)

En la acción `Expediente(long id)`, antes del `return View(paciente)`:

```csharp
var tratamientos = await _citaService.ConsultarTratamientos();
ViewBag.Tratamientos = (tratamientos.IsSuccess && tratamientos.Response != null) ? tratamientos.Response : new List<Tratamiento>();

var tipos = await _tipoConsultaService.ConsultarTiposConsulta();
ViewBag.TiposConsulta = (tipos.IsSuccess && tipos.Response != null) ? tipos.Response : new List<TipoConsulta>();
```

Declarar e instanciar el campo `private readonly TipoConsultaService _tipoConsultaService = new TipoConsultaService(httpClientConnection);` (el `using ConsultorioMedicoMVC.Services;` ya existe). `_citaService` ya está presente.

## Interfaces / Contratos

Sin cambios. Se reutilizan: `POST Cita/GuardarOActualizarCita` (binding `Cita`), `POST Consulta/GuardarOActualizarConsulta` (binding `Consulta`), SPs `sp_nh_SaveCita`/`sp_nh_SaveConsulta`, y los servicios de catálogos existentes. `name` duplicados entre los dos `<form>` no colisionan (validate por form, un solo `serialize()` por POST).

## Testing / Verificación

| Capa | Qué verificar | Enfoque |
|------|---------------|---------|
| Build | Compilación + Razor C# 5 | MSBuild Debug solución (`DESI.ConsultorioMedico.sln`) exit 0 + `aspnet_compiler` exit 0 |
| Manual | Ancho 100% | Inspección navegador: ambos grids alineados con el header al cargar (Tab 1) y al hacer clic en Tab 2 (sin encogerse) |
| Manual | Modal Nuevo Tratamiento | Paciente fijo (nombre solo lectura + hidden), fecha requerida bloquea, guardar → Swal + cierre + recarga SOLO Tab 1 |
| Manual | Modal Nueva Consulta | Ídem, recarga SOLO Tab 2; `Historial/Index` y `Cita/Index`/`Consulta/Index` intactos |

## Migration / Rollout

No requiere migración. Rollback: `git checkout` de `Expediente.cshtml` y `HistorialController.cs`.

## Riesgos

- Deriva de markup/JS duplicado (D7) → mitigación futura: extraer a partial si crece.
- jQuery Validate ignora `PacienteID` hidden → mitigado: valor fijo y regla `required` omitida (D3).
- Tab 2 aún no mida 100% → mitigado: `responsive` fuera + `width:100%` + `columns.adjust()` existente; verificar en navegador (D6).

## Open Questions

Ninguna que bloquee. Nota heredada del change previo: tratamientos de paquetes quedan fuera (change `paquetes-del-paciente`).
