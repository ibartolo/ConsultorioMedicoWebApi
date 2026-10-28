# Design: Expediente / Historial Clínico del Paciente

## Contexto

Vista consolidada de solo lectura del expediente de un paciente, accesible desde el grid de Pacientes. Reutiliza el flujo MVC→WebApi→SP ya existente. Tratamientos históricos se **derivan** de Cita+Consulta (no existe tabla de log; `TratamientoID` es FK nullable en ambas). `Historial/Index` queda intacto. Cambio aditivo; sin migración de datos.

## Decisiones de Arquitectura

| # | Decisión | Rationale | Alternativa rechazada |
|---|----------|-----------|----------------------|
| D1 | Tratamientos **derivados** de Cita+Consulta vía SP `sp_nh_GetTratamientosPorPaciente` (UNION + JOIN Tratamiento) | Fiel al modelo WinForms (§6.4/§6.11); no existe log de tratamientos | Tabla de log nueva (esquema) |
| D2 | DTO standalone `TratamientoHistoricoDTO` (no hereda entidad) | No existe entidad base; replica estilo `RecetaDTO` | Heredar de `Cita`/`Consulta` (mezcla campos) |
| D3 | Nombre del paciente se compone en la **vista** desde `Paciente` plano (Nombre + apellidos) | `api/Paciente/{id}` ya devuelve `Paciente`; reutiliza `PacienteService.ObtenerPacientePorId` sin endpoint nuevo | Usar `List` para obtener `NombreCompleto` (costoso) / alterar endpoint existente |
| D4 | Drill-down a receta con `window.open('Consulta/Receta/{id}', '_blank')` | `Receta.cshtml` es server-side imprimible; modal exigiría reimplementarla en AJAX | Modal AJAX |
| D5 | Tabs Bootstrap 5 `nav-tabs` nativo (ya en `_Layout`), Tabs 3/4 con `d-none` | Sin patrón previo; Bootstrap bundle ya cargado; evita UI muerta | JS custom |
| D6 | WebApi/MVC nuevos con naming simétrico `Historial` (Controller/Service/DbWrapper/HttpClientConnection) | Espeja patrón de `Consulta`/`Anexo`; un servicio por dominio | Reutilizar `ConsultaService`/`CitaService` para tratamientos (mezcla responsabilidades) |
| D7 | Guard `pacienteId <= 0` en `HistorialService` WebApi → `IsSuccess=false` + mensaje | Cumple spec "Id inválido"; la convención es **HTTP 200 siempre** (no 400) | HTTP 400 (rompe convención) |
| D8 | `Id` del DTO = Id de la fila origen (Cita.Id/Consulta.Id), informativo | No hay acción sobre filas del grid de tratamientos | Clave compuesta (innecesaria) |

## Diagramas de Secuencia

**Flujo 1 — Apertura del expediente (server-side):**

```
Paciente/Index (click) ──> HistorialController.Expediente(id)
                              └─> PacienteService.ObtenerPacientePorId(id)
                                    └─> HttpClientConnection → api/Paciente/{id} (ModelResponse<Paciente>)
                              └─> return View(paciente) → Views/Historial/Expediente.cshtml
```

**Flujo 2 — Carga de tratamientos (Tab 1, AJAX):**

```
Expediente.cshtml (DataTable init)
  └─> GET /Historial/ConsultarTratamientosPorPaciente?pacienteId={id}  [MVC]
        └─> HistorialService.ConsultarTratamientosPorPaciente(id)
              └─> HttpClientConnection → api/Historial/TratamientosPorPaciente/{id}
                    └─> HistorialService (WebApi) → DbWrapper.ObtenerTratamientosPorPaciente
                          └─> sp_nh_GetTratamientosPorPaciente (UNION Cita+Consulta)
        <── JSON ModelResponse<List<TratamientoHistoricoDTO>>
```

**Flujo 3 — Drill-down a receta (Tab 2, doble clic):**

```
Expediente.cshtml (dblclick fila consulta)
  └─> window.open('Consulta/Receta/' + id, '_blank')
        └─> ConsultaController.Receta(id) → api/Consulta/Receta/{id} → RecetaDTO (imprimible)
```

## Detalle por Capa

### Entities (`ConsultorioMedicoEntities`)
- **Nuevo** `Catalogos/TratamientoHistoricoDTO.cs` — POCO standalone (estilo `RecetaDTO`):
  - `int Id`, `DateTime? Fecha`, `string TratamientoNombre`, `string Origen`, `string Comentarios`.
- `.csproj`: `<Compile Include="Catalogos\TratamientoHistoricoDTO.cs" />`.

### WebApi (`ConsultorioMedicoWebApi`)
- **Nuevo** `DAL/DbWrapper.Historial.cs` (partial `DbWrapper`): método `ModelResponse<List<TratamientoHistoricoDTO>> ObtenerTratamientosPorPaciente(long pacienteId, string usuario)` → `GetObjects("sp_nh_GetTratamientosPorPaciente", mapper, CommandType.StoredProcedure, new[] { new SqlParameter("@IdPaciente", pacienteId) })`. Mapper con `Convert.IsDBNull` para `Fecha`.
- **Nuevo** `Services/HistorialService.cs`: `new DbWrapper()`; try/catch + Serilog; guard `if (pacienteId <= 0) return IsSuccess=false`.
- **Nuevo** `Controllers/HistorialController.cs`: `[Authorize] [RoutePrefix("api/Historial")]`, hereda `BaseController`, `_historialService = new HistorialService()`. Endpoint:
  - `[HttpGet, Route("TratamientosPorPaciente/{pacienteId:long}")]` → `ModelResponse<List<TratamientoHistoricoDTO>>` → `_historialService.ObtenerTratamientosPorPaciente(pacienteId, User.Identity.Name)`.
- `.csproj`: 3 `<Compile>` (controller, service, DbWrapper partial).

### MVC (`ConsultorioMedicoMVC`)
- **Nuevo** `DAL/HttpClientConnection.Historial.cs` (partial): `Task<ModelResponse<List<TratamientoHistoricoDTO>>> ObtenerTratamientosPorPaciente(long pacienteId)` → `RequestAsync<...>("api/Historial/TratamientosPorPaciente/" + pacienteId, HttpMethod.Get, null, token.Token.access_token)`.
- **Nuevo** `Services/HistorialService.cs`: constructor `(HttpClientConnection httpClient)`; `ConsultarTratamientosPorPaciente(long pacienteId)`.
- **Modificar** `Controllers/HistorialController.cs`: añadir `_historialService = new HistorialService(httpClientConnection)`; reorganizar en `#region Views`/`#region Data Access` (como `PacienteController`):
  - `Views`: `public async Task<ActionResult> Expediente(long id)` → `var p = await _pacienteService.ObtenerPacientePorId(id); if (p == null) return RedirectToAction("Index", "Paciente"); return View(p);`
  - `Data Access`: `public async Task<string> ConsultarTratamientosPorPaciente(long pacienteId)` → `JsonConvert.SerializeObject(await _historialService.ConsultarTratamientosPorPaciente(pacienteId))`.
- **Nuevo** `Views/Historial/Expediente.cshtml` (`@model ConsultorioMedicoEntities.Catalogos.Paciente`, BOM UTF-8, Razor C# 5): `nav-tabs` + `tab-content`; Tab1 panel (componer `@Model.Nombre @Model.ApellidoPaterno @Model.ApellidoMaterno` + Email/Tel/Sexo/Edad/FechaNacimiento) + DataTable tratamientos (AJAX `ConsultarTratamientosPorPaciente`, cols Fecha/TratamientoNombre/Origen/Comentarios); Tab2 DataTable consultas (AJAX existente `ConsultarConsultasPorPaciente`, cols Id/FechaConsulta/TipoConsultaNombre/Descripcion) con `row` dblclick → `window.open`; Tab3/Tab4 `d-none`. Inyectar `@Model.Id` como `pacienteId` en `data` de los AJAX.
- **Modificar** `Views/Paciente/Index.cshtml`: en columna Acciones renderizar 2 botones (editar + `<a href="@Url.Action("Expediente","Historial")/{id}">Expediente Clínico</a>`).
- `.csproj`: 2 `<Compile>` (HttpClientConnection partial, HistorialService) + `<Content Include="Views\Historial\Expediente.cshtml" />`.

### SQL (`sql/sp_nh_Historial.sql`)
- **Nuevo** — solo `CREATE` con `IF OBJECT_ID` (estilo `sp_nh_Cita.sql`), sin DROP/ALTER.

## Contrato del SP

**`sp_nh_GetTratamientosPorPaciente`** — Parámetro: `@IdPaciente INT`.

```sql
SELECT c.Id, c.FechaCita AS Fecha, t.Nombre AS TratamientoNombre,
       'Cita' AS Origen, c.Comentarios
FROM dbo.Cita c LEFT JOIN dbo.Tratamiento t ON c.TratamientoID = t.Id
WHERE c.PacienteID = @IdPaciente AND c.Estatus = 1
UNION
SELECT co.Id, co.FechaConsulta AS Fecha, t.Nombre AS TratamientoNombre,
       'Consulta' AS Origen, co.Comentarios
FROM dbo.Consulta co LEFT JOIN dbo.Tratamiento t ON co.TratamientoID = t.Id
WHERE co.PacienteID = @IdPaciente AND co.Estatus = 1
ORDER BY Fecha DESC;
```

Columnas: `Id (int), Fecha (datetime), TratamientoNombre (nvarchar), Origen (nvarchar), Comentarios (nvarchar)`. `UNION` (no `UNION ALL`) deduplica filas idénticas; `Id` evita colisiones origen-cruzado.

## Verificación

- **MSBuild** solución Debug (`DESI.ConsultorioMedico.sln`) → exit 0.
- **aspnet_compiler** precompilando vistas (valida `Expediente.cshtml` Razor C# 5 y `Paciente/Index.cshtml`).
- Contratos: endpoint responde `ModelResponse<T>` HTTP 200 (camelCase), mensajes español; grid vacío sin error; drill-down abre pestaña nueva.

## Riesgos

- `UNION` con fechas/tratamientos iguales en Cita y Consulta deduplicado solo si `Id` coincide (improbable); `Id` difiere entre tablas.
- `FechaConsulta` es datetime único (sin columna Hora separada) — la "hora" sale del propio datetime.
- El SP **no se ejecuta** en BD productiva salvo aprobación; solo se versiona el script.
- Pregunta abierta (→ change 2): ¿incluir tratamientos de paquetes? Requiere capa `PacientePaquete`/`TratamientoPaquete`.
