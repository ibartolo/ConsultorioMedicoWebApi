# Design: Calendario de Citas — Estados, Colores y Eliminación

## Contexto

Change `calendario-citas-estados`. Cuatro frentes: (1) fix de sintaxis JS que rompe el calendario de Consultas; (2) pintar eventos del calendario de Citas según `Estado` + tooltip; (3) baja lógica de Cita (`Estatus=0`) con botón "Eliminar" (patrón de borrado nuevo, hoy inexistente); (4) ejecutar `sp_nh_GetTratamientosPorPaciente` en BD productiva. Fuentes: `proposal.md` y `specs/cita/spec.md`. Capability nueva `cita`; los fixes de Consulta/historial son bugfix/operación.

Hallazgo clave de serialización: la WebApi serializa camelCase (`CamelCasePropertyNamesContractResolver` en `WebApiConfig.cs`), pero el MVC re-serializa con `JsonConvert.SerializeObject` (Newtonsoft default = PascalCase). Por eso el JS del calendario ya lee `c.PacienteNombre`, `c.Estado`, etc. en PascalCase. Los helpers nuevos usan las mismas claves PascalCase (`c.Estado`, `c.PacienteNombre`, `c.TratamientoNombre`).

## Decisiones

### D1 — Fix de sintaxis en Consulta/Index.cshtml

**Choice**: `ConsultorioMedicoMVC/Views/Consulta/Index.cshtml` línea 119: `initialView: 'timeGridWeek` → `initialView: 'timeGridWeek',` (agregar la coma final). Confirmado leyendo el archivo (falta la coma, rompe `slotDuration` y todo el `<script>`).

**Rationale**: bug de sintaxis JS; el calendario de Consultas nunca renderiza. Sin coma, `slotDuration` es un statement inválido. Cambio mínimo, sin impacto en datos.

### D2 — Colores aplicados en la construcción del evento (no en `eventDidMount`)

**Choice**: función `colorEstado(estado)` que retorna `{ backgroundColor, borderColor, textColor }`; se aplica en `events.push(...)` con los campos nativos `backgroundColor`/`borderColor`/`textColor`.

```js
function colorEstado(e) {
    if (e === 1) return { backgroundColor: '#FD7E14', borderColor: '#FD7E14', textColor: '#FFFFFF' };
    if (e === 2) return { backgroundColor: '#198754', borderColor: '#198754', textColor: '#FFFFFF' };
    if (e === 3) return { backgroundColor: '#DC3545', borderColor: '#DC3545', textColor: '#FFFFFF' };
    return { backgroundColor: '#6C757D', borderColor: '#6C757D', textColor: '#FFFFFF' }; // null/otro
}
```

**Alternativas**: pintar en `eventDidMount` vía `info.el.style.*`. Rechazado: FullCalendar 6.1.15 soporta `backgroundColor`/`borderColor`/`textColor` nativos, válidos en `timeGridWeek` y `dayGridMonth`, sin tocar el DOM.

**Rationale**: declarativo, coherente con la construcción actual del evento (`events.push`), y colores legibles (texto blanco sobre fondos oscuros).

### D3 — Tooltip en `eventDidMount` con `nombreEstado()`

**Choice**: `eventDidMount` setea `info.el.title` (tooltip nativo del navegador). Helper `nombreEstado(e)`: `1→Pendiente`, `2→Confirmada`, `3→Cancelada`, default `'Sin estado'`.

```js
function nombreEstado(e) {
    if (e === 1) return 'Pendiente';
    if (e === 2) return 'Confirmada';
    if (e === 3) return 'Cancelada';
    return 'Sin estado';
}
// eventDidMount:
eventDidMount: function (info) {
    var p = info.event.extendedProps;
    var t = 'Estado: ' + nombreEstado(p.Estado);
    if (p.PacienteNombre) t += ' - ' + p.PacienteNombre;
    if (p.TratamientoNombre) t += ' - ' + p.TratamientoNombre;
    info.el.title = t;
}
```

**Reconciliación**: el task prompt sugería `default → ''`, pero la spec (`specs/cita/spec.md`) obliga tooltip "Sin estado" para `Estado` null. Gana la spec: default `'Sin estado'`.

**Rationale**: `eventDidMount` permite HTML nativo del navegador sin plugin de tooltip (ver proposal Notas). Incluye paciente/tratamiento (SHOULD de la spec).

### D4 — Tipo de respuesta de Eliminar: `ModelResponse<bool>` (reconciliación)

**Choice**: `ModelResponse<bool>` con semántica "¿se eliminó?" (`IsSuccess=true` + `Response=true` al éxito).

**Alternativas**: `ModelResponse<int>` (proposal) exponiendo el rowcount/id.

**Rationale**: la spec es fuente de verdad y obliga `ModelResponse<bool>` (`Response=true`). Además el frontend solo necesita éxito/fallo (ya en `IsSuccess`+`Message`); el `int` (rowcount) no aporta semántica al consumidor. El rowcount sigue usándose **internamente** en el DbWrapper como chequeo de existencia (ver D6).

### D5 — Paso del `id` MVC→WebApi: route param `Eliminar/{id:int}` (reconciliación)

**Choice**: WebApi `[HttpPost, Route("Eliminar/{id:int}")] EliminarCita(int id)`. MVC DAL: `RequestAsync<bool>("api/Cita/Eliminar/" + id, HttpMethod.Post, null, token)`.

**Alternativas**: (a) query `?id=` con ruta plana `Eliminar`; (b) `[FromBody] int id` con body JSON `5`.

**Rationale**: en `CitaController` (WebApi) los ids ya se pasan por ruta (`{id:long}`, `PorPaciente/{pacienteId:long}`); un tipo simple se bindea nativo desde URI sin el quirk de `[FromBody]`. `RequestAsync` con `content=null` no setea body, lo que es válido para POST por ruta. La desviación del literal `api/Cita/Eliminar` de la spec es aceptable: el requisito es "recibe el id", preservado. (Query string también hubiera funcionado pero no es el patrón de este controller.)

### D6 — SP `sp_nh_DeleteCita` sin `SELECT @@ROWCOUNT` (reconciliación)

**Choice**: SP mínimo `UPDATE dbo.Cita SET Estatus = 0 WHERE Id = @Id;` con `SET NOCOUNT ON`. El DbWrapper usa `ExecuteNonQuery`, cuyo retorno (`int` = filas afectadas por el UPDATE) sirve de chequeo de existencia: `0` → "Cita no encontrada", `>0` → éxito.

**Alternativas**: `SELECT @@ROWCOUNT` + `ExecuteScalar`. Rechazado: `ExecuteNonQuery` ya devuelve el rowcount del UPDATE (el `SELECT` sobra y `SET NOCOUNT ON` no altera el retorno de `ExecuteNonQuery`).

**Rationale**: spec solo exige el `UPDATE`; el rowcount se obtiene gratis de `ExecuteNonQuery`. Evita un round-trip de scalar innecesario.

### D7 — Ubicación del SP: anexar a `sql/sp_nh_Cita.sql`

**Choice**: anexar el 4º bloque `sp_nh_DeleteCita` (CREATE + `IF OBJECT_ID('dbo.sp_nh_DeleteCita','P') IS NULL`) al final de `sql/sp_nh_Cita.sql`, y actualizar el comentario de cabecera para listarlo.

**Rationale**: la convención agrupa los SPs por módulo en un solo archivo (`sp_nh_Cita.sql` ya tiene 3 SPs; `sp_nh_Historial.sql` los de Historial). Un archivo separado fragmentaría el módulo sin ganancia.

### D8 — Browser→MVC: `$.ajax` con `data: { id: X }` (no serialize)

**Choice**: `eliminarCita()` hace `$.ajax({ url: eliminarUrl, type: 'POST', data: { id: $('#Id').val() }, ... })`. MVC `EliminarCita(long id)` lo bindea por form-data.

**Rationale**: el form ya tiene `<input hidden name="Id">`; un `data:{id}` mínimo evita enviar `PacienteID`/`Estado`/etc. y no dispara el jQuery Validate. Consistente con el patrón form-data del proyecto (nunca JSON.stringify al browser→MVC).

### D9 — Tipos de `id`: `int` (WebApi) vs `long` (MVC)

**Choice**: WebApi (DbWrapper/Service/Controller) usa `int id` (coincide con `Cita.Id` (`int`) y `@Id INT` del SP; ruta `{id:int}`). MVC (DAL/Service/Controller) usa `long id` (coincide con `Nuevo(long id)`/`ObtenerCitaPorId(long id)` del controller MVC y con el prompt).

**Rationale**: sin casts en la frontera SQL (`int`→`INT`) y sin divergencia de la convención MVC existente. El paso por URL/form-data es textual, sin pérdida.

## Diagramas de secuencia

### Eliminar cita (click → SweetAlert → POST MVC → HttpClient → WebApi → SP → refetch)

```
Usuario ──click "Eliminar"──▶ eliminarCita() [JS, Cita/Index.cshtml]
  eliminarCita ──Swal.fire(warning, showCancelButton)──▶ Usuario
  (confirma) ──$.ajax POST data:{id}──▶ MVC CitaController.EliminarCita(long id)
     ──await──▶ MVC CitaService.EliminarCita(long id)
        ──RequestAsync<bool>("api/Cita/Eliminar/{id}", POST, null)──▶ WebApi CitaController.EliminarCita(int id)
           ──▶ WebApi CitaService.EliminarCita(int id, User.Identity.Name)  [valida id<=0]
              ──▶ DbWrapper.EliminarCita(int id) ── ExecuteNonQuery ──▶ sp_nh_DeleteCita @Id
                 UPDATE dbo.Cita SET Estatus=0 WHERE Id=@Id   ◀── rowcount (0|1) ──
              ◀── ModelResponse<bool> ──
           ◀── ModelResponse<bool> ──
        ◀── ModelResponse<bool> (JSON camelCase→deser) ──
     ◀── JsonConvert.SerializeObject (string) ──
  eliminarCita ──Swal éxito──▶ cerrar modal + calendar.refetchEvents()  [listado filtra Estatus=1]
```

### Colores + tooltip (render del calendario)

```
calendar.refetchEvents() ──$.ajax GET listUrl──▶ MVC ConsultarTodasCitas()
   ◀── ModelResponse<List<CitaDTO>> (JSON PascalCase) ──
 events.push({..., backgroundColor/borderColor/textColor: colorEstado(c.Estado), extendedProps: c})
 FullCalendar render → eventDidMount(info) → info.el.title = 'Estado: ' + nombreEstado(Estado) + paciente/tratamiento
```

## Detalle por capa

| Capa | Archivo | Cambio exacto |
|------|---------|---------------|
| SQL | `sql/sp_nh_Cita.sql` | Anexar `sp_nh_DeleteCita` (`@Id INT` → `UPDATE dbo.Cita SET Estatus=0 WHERE Id=@Id;`, `SET NOCOUNT ON`, CREATE + `IF OBJECT_ID`). Actualizar cabecera. |
| SQL (op) | `sql/sp_nh_Historial.sql` | Ejecutar contra BD productiva (`cCon` en Web.config WebApi). CREATE idempotente. |
| WebApi DAL | `ConsultorioMedicoWebApi/DAL/DbWrapper.Cita.cs` | `public ModelResponse<bool> EliminarCita(int id, string usuario)` → `ExecuteNonQuery("sp_nh_DeleteCita", ..., new[]{ new SqlParameter("@Id", id) })`; `filas<=0`→`IsSuccess=false, "Cita no encontrada."`; else `Response=true`. |
| WebApi Svc | `ConsultorioMedicoWebApi/Services/CitaService.cs` | `public ModelResponse<bool> EliminarCita(int id, string usuario)`; valida `id<=0`→error; try/catch→`ModelResponse<bool>` con mensajes en español. |
| WebApi Ctrl | `ConsultorioMedicoWebApi/Controllers/CitaController.cs` | `[HttpPost, Route("Eliminar/{id:int}")] public ModelResponse<bool> EliminarCita(int id) => _citaService.EliminarCita(id, User.Identity.Name);` |
| MVC DAL | `ConsultorioMedicoMVC/DAL/HttpClientConnection.Cita.cs` | `public async Task<ModelResponse<bool>> EliminarCita(long id) => await RequestAsync<bool>("api/Cita/Eliminar/" + id, HttpMethod.Post, null, token.Token.access_token);` |
| MVC Svc | `ConsultorioMedicoMVC/Services/CitaService.cs` | `public async Task<ModelResponse<bool>> EliminarCita(long id) => await _httpClient.EliminarCita(id);` |
| MVC Ctrl | `ConsultorioMedicoMVC/Controllers/CitaController.cs` | Región Data Access: `[HttpPost] public async Task<string> EliminarCita(long id) => JsonConvert.SerializeObject(await _citaService.EliminarCita(id));` |
| MVC View | `ConsultorioMedicoMVC/Views/Cita/Index.cshtml` | `colorEstado()`/`nombreEstado()`; aplicar colores en `events.push`; `eventDidMount` (tooltip); botón `#btnEliminarCita` (btn-danger) en footer; `var eliminarUrl`; `eliminarCita()` (Swal confirm `confirmButtonColor:'#DC3545'` → POST → Swal éxito → `modal('hide')` + `refetchEvents()`); `abrirModalCita` hace `.toggle(c && c.Id>0)` del botón. |
| MVC View | `ConsultorioMedicoMVC/Views/Consulta/Index.cshtml` | Línea 119: `initialView: 'timeGridWeek',` (agregar coma). |

Registro en `.csproj`: los archivos parciales existentes (`DbWrapper.Cita.cs`, `HttpClientConnection.Cita.cs`, `CitaService.cs` MVC/WebApi, `CitaController.cs`) ya están incluidos; solo se editan. No hay archivos nuevos.

## Consideraciones de verificación

- **Build**: `& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe' 'DESI.ConsultorioMedico.sln' /t:Build /p:Configuration=Debug` (exit 0) + `aspnet_compiler` para precompilar vistas Razor (ver PLAYBOOK §9).
- **BD productiva (Smarterasp)**: ejecutar `sql/sp_nh_Cita.sql` (crea/omite `sp_nh_DeleteCita`) y `sql/sp_nh_Historial.sql` (crea/omite `sp_nh_GetTratamientosPorPaciente`). Verificar con `SELECT name FROM sys.procedures WHERE name IN ('sp_nh_DeleteCita','sp_nh_GetTratamientosPorPaciente')`. Ambos son CREATE idempotentes (no dañan datos; el delete no se invoca solo).
- **Manual/E2E**: (a) calendario Consultas renderiza; (b) calendario Citas pinta por estado (1 naranja / 2 verde / 3 rojo / null gris) y tooltip "Estado: …"; (c) "Eliminar" visible solo al editar (`Id>0`), oculto en nueva; confirmar → la cita desaparece (`refetchEvents`, listado filtra `Estatus=1`); cancelar → sin POST; (d) grid "Tratamientos Históricos" deja de estar vacío.
- **Rollback**: UI y backend = `git checkout` de las 2 vistas y 6 `.cs`; SPs idempotentes sin efecto en listados (`Estatus=1`).

## Open Questions

Ninguna. Todas las decisiones (tipo de respuesta, paso de `id`, ubicación del SP, `SELECT @@ROWCOUNT`, nombres de propiedades PascalCase) quedaron resueltas y documentadas.
