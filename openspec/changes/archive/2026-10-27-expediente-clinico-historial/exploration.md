# Exploration: Expediente / Historial Clínico del paciente

## Estado Actual

### MVC (ConsultorioMedicoMVC)
- `Views/Paciente/Index.cshtml` — DataTable con UNA sola acción por fila (lápiz editar → `Paciente/Nuevo/{id}`). **NO existe botón "Expediente Clínico"**.
- `Views/Historial/Index.cshtml` — selector de paciente (`ViewBag.Pacientes`) + 3 DataTables sueltas (Citas/Consultas/Anexos). **NO** es 4 pestañas, sin panel de info del paciente, sin paquetes, sin drill-down.
- `Controllers/HistorialController.cs` — `Index()` + 3 acciones DataAccess (`ConsultarCitasPorPaciente`, `ConsultarConsultasPorPaciente`, `ConsultarAnexosPorPaciente`). Reutiliza `CitaService`, `ConsultaService`, `AnexoService`, `PacienteService` (MVC).
- `Views/Consulta/Receta.cshtml` — vista server-side imprimible (`RecetaDTO`), abierta desde `ConsultaController.Receta(long id)` → link `Consulta/Receta/{id}`. Botón "Volver" apunta a `Consulta/Index` (no a Historial).
- `Views/Consulta/Index.cshtml` + `Views/Cita/Index.cshtml` — patrón **FullCalendar + modal Bootstrap** para crear/editar. **No existe patrón de tabs en TODO el proyecto** (grep `nav-tabs|tab-pane` = 0 resultados). Bootstrap 5 bundle ya cargado en `_Layout.cshtml` (soporta `nav-tabs` nativo).
- `_Layout.cshtml` — sidebar; ítem "Historial" → `Historial/Index`. No hay entrada de menú para "Expediente".

### WebApi (ConsultorioMedicoWebApi)
- `Controllers/ConsultaController.cs` — rutas: `List`, `{id:long}` → `Consulta` (SIN nombres), `Guardar`, `Receta/{consultaId:long}`, `PorPaciente/{pacienteId:long}`.
- `DAL/DbWrapper.Consulta.cs` — `sp_nh_GetConsultaById` devuelve la entidad `Consulta` plana (sin `PacienteNombre`/`TipoConsultaNombre`). `ObtenerRecetaPorConsulta` reutiliza SP `GetRecetaByConsulta` (devuelve Paciente, FechaConsulta, TipoConsulta, Medicamento, Consulta, ProximaConsulta). `ObtenerConsultasPorPaciente` reutiliza `GetAllConsultaByPacienteID` → `ConsultaDTO` con `TipoConsultaNombre` (TC.Nombre) y `DescripcionReceta`, pero **SIN `TratamientoNombre`** (el SP no hace JOIN a Tratamiento).
- `DAL/DbWrapper.Cita.cs` — `ObtenerCitasPorPaciente` reutiliza `GetAllCitaByPacienteID` → `CitaDTO` con `TratamientoNombre` (T.Nombre) y `Comentarios`.
- `Controllers/PacienteController.cs` — `List`, `{id:long}` (devuelve `Paciente`, sin `NombreCompleto`), `Guardar`.

### Entities (ConsultorioMedicoEntities)
- `Catalogos/Consulta.cs` (POCO plano), `ConsultaDTO.cs` (+PacienteNombre, TipoConsultaNombre), `RecetaDTO.cs`, `CitaDTO.cs` (+PacienteNombre, TratamientoNombre), `Paciente.cs` (+BaseObject auditoría), `PacienteDTO.cs` (+NombreCompleto).

### Datos (script.sql + docs)
- Tablas puente existen en BD: `PacientePaquete` (Id, PacienteID, PaqueteID), `PagoPaquete` (Id, Pago, FechaPago, PacientePaqueteID), `TratamientoPaquete` (Id, PaqueteID, TratamientoID), `CitaPaquete` (Id, CitaID, PacientePaqueteID). **Cero referencias C#** a estas 4 tablas.
- SPs de paquetes en `script.sql` SIN wrapper C#: `GetAllPagoPaqueteByPacientePaqueteIdDT` (@pacientepadoID → Precio, Pagado, Adeuda), `GetAllTratamiendoByPaqueteID` (@paqueteID → TratamientoID AS ID, Nombre), `GetTratamientoPorPauqte`, `SaveOrUpdateTratamientoPaquete`.
- `docs/flujo-aplicacion-winforms.md` §6.4 (`VDatosDelPaciente` = ficha con datos+citas+consultas+anexos), §6.11 (`CatalogoHistorial` = citas+consultas+paquetes), §7 (catálogo SPs), §8 (SavePacientePaquete/SavePagoPaquete → NHibernate, no hay SP).

## Brechas (qué falta)

1. **Botón "Expediente Clínico"** en `Paciente/Index.cshtml` → link a la nueva vista.
2. **Vista de expediente con 4 pestañas** (hoy Historial son 3 DataTables sueltas sin tabs).
3. **Panel de información del paciente** en Tab 1 (hoy no se muestra).
4. **Grid de "tratamientos históricos" del paciente** (Tab 1) — **no existe log ni endpoint**. `GetAllConsultaByPacienteID` no trae `TratamientoNombre`.
5. **Drill-down de consulta** (doble clic en Tab 2) — hoy no hay interacción; reutilizable vía `Consulta/Receta/{id}`.
6. **Tab 4 Paquetes** — TODA la capa de asociación falta (va en change 2). En este change: oculta.
7. **Endpoints nuevos**: `TratamientosPorPaciente` (derivado de Cita+Consulta). Opcional: detalle enriquecido de consulta (cubierto por `Receta`).
8. **SP nuevo**: `sp_nh_GetTratamientosPorPaciente` (UNION Cita + Consulta con JOIN Tratamiento).

## Decisiones de diseño recomendadas

1. **Tabs**: Bootstrap 5 `nav-tabs` nativo (ya cargado). No hay patrón previo; se introduce en la nueva vista `Views/Historial/Expediente.cshtml`.
2. **Ruta de la vista**: nueva acción `HistorialController.Expediente(long id)` → `Views/Historial/Expediente.cshtml`. El botón del grid de Paciente apunta a `/Historial/Expediente/{id}`. El ítem de menú "Historial" queda en `Index` (selector) intacto.
3. **Drill-down de consulta**: reutilizar la vista imprimible `Consulta/Receta/{id}` (ya trae fecha/hora, paciente, tipo, receta). Abrir en **nueva pestaña** (`window.open`) para no perder el contexto del expediente y porque su botón "Volver" va a `Consulta/Index`. Sin endpoint nuevo.
4. **Grid de tratamientos (Tab 1)**: **DERIVAR** de Cita+Consulta (no existe tabla de log de tratamientos por paciente; `TratamientoID` solo es FK nullable en `Cita` y `Consulta`; `TratamientoPaquete` es composición del catálogo Paquete, no historial). Crear SP `sp_nh_GetTratamientosPorPaciente` que haga UNION (Cita+Consulta) con JOIN a `Tratamiento`, devolviendo fecha, nombre del tratamiento, origen (Cita/Consulta) y comentarios. Es el enfoque fiel al modelo WinForms (§6.4/§6.11 leen Cita+Consulta, no un log).
5. **Tab 3 Archivos**: OCULTA (sin subir archivos; el módulo Anexo ya existe y se muestra en el selector del Historial actual).
6. **Tab 4 Paquetes**: **OCULTA en este change** (placeholder `d-none`). La capa `PacientePaquete`/`PagoPaquete`/`TratamientoPaquete`/`CitaPaquete` completa va en change 2 `paquetes-del-paciente`. Evitar UI muerta.
7. **Botones "Nuevo Tratamiento" / "Nueva Consulta"**: **diferir al change 2**. Son atajos a los flujos existentes de Cita/Consulta (crear con paciente pre-seleccionado). Requieren que `Cita/Consulta` acepten `pacienteId` por query-string como preselección (mejora menor). Mantener el change 1 enfocado en el expediente de solo lectura.

## Archivos a tocar (estimado)

### Entities
- `ConsultorioMedicoEntities/Catalogos/TratamientoHistoricoDTO.cs` (NUEVO: Id, Fecha, TratamientoNombre, Origen, Comentarios) + registro en `.csproj`.

### WebApi
- `ConsultorioMedicoWebApi/Controllers/HistorialController.cs` (NUEVO, `[Authorize] [RoutePrefix("api/Historial")]`, `TratamientosPorPaciente/{id}`).
- `ConsultorioMedicoWebApi/Services/HistorialService.cs` (NUEVO).
- `ConsultorioMedicoWebApi/DAL/DbWrapper.Historial.cs` (NUEVO, método `ObtenerTratamientosPorPaciente` → SP nuevo). Registros en `.csproj`.

### MVC
- `ConsultorioMedicoMVC/Controllers/HistorialController.cs` — añadir `Expediente(long id)` (carga paciente + tratamientos/consultas) y `ConsultarTratamientosPorPaciente` (DataAccess).
- `ConsultorioMedicoMVC/Services/HistorialService.cs` (NUEVO) o método en servicio existente.
- `ConsultorioMedicoMVC/DAL/HttpClientConnection.Historial.cs` (NUEVO) — llamada `api/Historial/TratamientosPorPaciente/{id}`.
- `ConsultorioMedicoMVC/Views/Historial/Expediente.cshtml` (NUEVO) — tabs + panel info + grids.
- `ConsultorioMedicoMVC/Views/Paciente/Index.cshtml` — añadir botón "Expediente Clínico".
- Registros `.cshtml`/`.cs` en `.csproj`.

### SQL
- `sql/sp_nh_Historial.sql` (NUEVO) — `sp_nh_GetTratamientosPorPaciente` (solo `CREATE` con `IF OBJECT_ID`).

## Riesgos / preguntas abiertas

- **`CitaPaquete` tiene FKs bugueadas** (heredadas, §9.6 del doc) — NO tocar; sólo afecta al change 2.
- **`GetAllConsultaByPacienteID` no devuelve `TratamientoNombre`** → obliga al SP UNION nuevo (no se puede `ALTER` el SP existente; convención solo `CREATE`).
- **`FechaConsulta` es datetime único** (sin columna Hora separada); el "hora" se muestra desde el datetime.
- **Drill-down**: la vista `Receta.cshtml` es server-side e imprimible; abrirla como modal implicaría reimplementarla en AJAX. Recomendado: `window.open` a la vista existente.
- **`api/Paciente/{id}` devuelve `Paciente` sin `NombreCompleto`** — componer el nombre completo en el panel desde `Nombre` + apellidos.
- **Razor C# 5** en `.cshtml` (no `?.` ni `$""`), BOM UTF-8 en archivos con acentos.
- **Pregunta abierta**: confirmar con el usuario si el "grid de tratamientos" debe incluir también tratamientos tomados de paquetes (eso solo es posible con la capa del change 2); en change 1 se deriva solo de Cita+Consulta.

## Ready for Proposal
Sí — el cambio está bien acotado como expediente de solo lectura (tabs + panel + 2 grids + drill-down), con Tab 3 y Tab 4 ocultas y botones diferidos al change 2. El orquestador puede proceder a `sdd-propose` con esta exploración.
