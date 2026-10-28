# Design: Paquetes del Expediente — Solo Lectura (Tab 4)

## Contexto

La Tab 4 "Paquetes" del expediente (`Historial/Expediente/{id}`) está oculta (`d-none`, línea 36) y vacía (línea 111). Se habilita en **solo lectura** replicando la pestaña Paquetes del `CatalogoHistorial` WinForms: grid de paquetes adquiridos (`PacientePaquete` JOIN `Paquete`) y, al hacer click en uno, grid de citas-tratamientos (`CitaPaquete` JOIN `Cita` LEFT JOIN `Tratamiento`). Flujo MVC → WebApi → SP (patrón `sp_nh_GetTratamientosPorPaciente`). Sin pagos, sin altas, sin tocar esquema/FKs.

## Arquitectura / Decisiones

| # | Decisión | Alternativas consideradas | Rationale |
|---|----------|---------------------------|-----------|
| D1 | **Archivo SQL dedicado `sql/sp_nh_PacientePaquete.sql`** con 2 SPs | Anexar a `sp_nh_Historial.sql` o `sp_nh_Paquete.sql` | `sp_nh_Historial.sql` cubre Cita/Consulta; `sp_nh_Paquete.sql` el CRUD del catálogo. El dominio puente (PacientePaquete/CitaPaquete) agrupa su propio archivo y coincide con lo que `paquetes-del-paciente` ampliará luego. |
| D2 | **DTOs con sufijo `Lectura`**: `PacientePaqueteLecturaDTO` y `CitaPaqueteLecturaDTO` | `PacientePaqueteDTO` / `CitaPaqueteDTO` (usados en la spec paralela y en el proposal futuro `paquetes-del-paciente`) | El change futuro `paquetes-del-paciente` ya reserva `PacientePaqueteDTO` {Id, Nombre, PacienteID, PacientePaqueteID} y `CitaPaqueteDTO`. Reusar esos nombres colisionaría (formas distintas). El sufijo `Lectura` es inequívoco y delimita este change. La spec se reconcilia en `sdd-archive`. |
| D3 | **Parámetros `int`** en SPs, DTOs, rutas `{id:int}` y acciones | `long` (como `TratamientosPorPaciente/{pacienteId:long}` existente) | `PacientePaquete.Id`, `Cita.Id` y los `@` son `INT`; `int` es el tipo exacto. Divergencia menor y sin impacto con el `long` existente. |
| D4 | **LEFT JOIN `Tratamiento`** + filtro `C.Estatus = 1` en el SP de citas | INNER JOIN Tratamiento | Preserva citas sin tratamiento (TratamientoNombre nulo). El filtro de estatus replica el WF. |
| D5 | **Recarga selectiva del grid de citas** vía variable JS + `citasPaqueteTable.ajax.reload()` | `ajax.url()` con query-string, o recargar ambos grids | Patrón análogo al `pacienteId` actual; solo se recarga el grid de citas, no toda la vista. |

## Diagramas de secuencia

**Carga de paquetes (apertura Tab 4):**

```
Expediente.cshtml → MVC HistorialController.ConsultarPaquetesPorPaciente(pacienteId)
      → HistorialService.ConsultarPaquetesPorPaciente → HttpClientConnection.Historial
      → GET api/Historial/PaquetesPorPaciente/{pacienteId}
      → WebApi HistorialController → HistorialService (guard id>0) → DbWrapper.Historial
      → SP sp_nh_GetPaquetesPorPaciente → ModelResponse<List<PacientePaqueteLecturaDTO>>
      → DataTables #paquetesTable (dataSrc IsSuccess ? Response : [])
```

**Click paquete → carga de citas:**

```
click fila #paquetesTable → row().data() → pacientePaqueteIdSeleccionado = data.Id (PacientePaquete.Id)
      → citasPaqueteTable.ajax.reload() (d.pacientePaqueteId = pacientePaqueteIdSeleccionado)
      → MVC ConsultarCitasPorPacientePaquete → GET api/Historial/CitasPorPacientePaquete/{id}
      → SP sp_nh_GetCitasPorPacientePaquete → ModelResponse<List<CitaPaqueteLecturaDTO>>
      → DataTables #citasPaqueteTable
```

## Detalle por capa

### SQL — `sql/sp_nh_PacientePaquete.sql` (nuevo)

Patrón idempotente **guard + CREATE** (`IF OBJECT_ID(...) IS NULL` + `sp_executesql`/CREATE; **sin DROP/ALTER**). Difieren de `sp_nh_Historial.sql` (DROP+CREATE) para no re-crear objetos existentes.

```sql
-- sp_nh_GetPaquetesPorPaciente @PacienteId INT
SELECT PP.Id, PP.PacienteID, PP.PaqueteID, P.Nombre AS PaqueteNombre
FROM PacientePaquete PP
INNER JOIN Paquete P ON P.Id = PP.PaqueteID
WHERE PP.PacienteID = @PacienteId AND P.Estatus = 1;

-- sp_nh_GetCitasPorPacientePaquete @PacientePaqueteId INT
SELECT C.Id, C.FechaCita, T.Nombre AS TratamientoNombre, C.Comentarios,
       CP.Id AS CitaPaqueteID, CP.PacientePaqueteID
FROM CitaPaquete CP
INNER JOIN Cita C ON C.Id = CP.CitaID
INNER JOIN PacientePaquete PP ON PP.Id = CP.PacientePaqueteID
LEFT JOIN Tratamiento T ON T.Id = C.TratamientoID
WHERE C.Estatus = 1 AND PP.Id = @PacientePaqueteId
ORDER BY C.FechaCita DESC;
```

### Entities — `ConsultorioMedicoEntities/Catalogos/` (2 nuevos)

- `PacientePaqueteLecturaDTO.cs`: `int Id` (= PacientePaquete.Id), `int PacienteID`, `int PaqueteID`, `string PaqueteNombre`.
- `CitaPaqueteLecturaDTO.cs`: `int Id` (= Cita.Id), `DateTime? FechaCita`, `string TratamientoNombre`, `string Comentarios`, `int CitaPaqueteID`, `int PacientePaqueteID`.

Registrar ambos `<Compile Include="Catalogos\...DTO.cs" />` en `ConsultorioMedicoEntities.csproj`.

### WebApi (solo editar existentes; no registrar en csproj)

- `Controllers/HistorialController.cs`:
  - `[HttpGet, Route("PaquetesPorPaciente/{pacienteId:int}")] ModelResponse<List<PacientePaqueteLecturaDTO>> ObtenerPaquetesPorPaciente(int pacienteId)` → `_historialService.ObtenerPaquetesPorPaciente(pacienteId, User.Identity.Name)`.
  - `[HttpGet, Route("CitasPorPacientePaquete/{pacientePaqueteId:int}")] ModelResponse<List<CitaPaqueteLecturaDTO>> ObtenerCitasPorPacientePaquete(int pacientePaqueteId)`.
- `Services/HistorialService.cs`: `ObtenerPaquetesPorPaciente(int, string)` y `ObtenerCitasPorPacientePaquete(int, string)` con guard `<= 0` → `ModelResponse` `IsSuccess=false` (HTTP 200, español; **no** 400 — patrón real de `ObtenerTratamientosPorPaciente`; la spec que dice "400" se reconcilia en archive).
- `DAL/DbWrapper.Historial.cs`: 2 métodos `GetObjects` con mapper inline y `SqlParameter("@PacienteId")` / `("@PacientePaqueteId")`. Mapper de citas usa `Convert.IsDBNull(r["FechaCita"]) ? (DateTime?)null : Convert.ToDateTime(...)`.

### MVC (solo editar existentes)

- `DAL/HttpClientConnection.Historial.cs`: `ObtenerPaquetesPorPaciente(int)` / `ObtenerCitasPorPacientePaquete(int)` → `RequestAsync<List<T>>("api/Historial/.../" + id, HttpMethod.Get, null, token.Token.access_token)`.
- `Services/HistorialService.cs`: `ConsultarPaquetesPorPaciente(int)` / `ConsultarCitasPorPacientePaquete(int)`.
- `Controllers/HistorialController.cs`: acciones Data Access `public async Task<string> ConsultarPaquetesPorPaciente(int pacienteId)` y `ConsultarCitasPorPacientePaquete(int pacientePaqueteId)` → `JsonConvert.SerializeObject(response)`.

### Vista — `Views/Historial/Expediente.cshtml` (editar; ya registrada)

- Quitar `d-none` del `<li>` Tab 4 (línea 36). Mantener `d-none` en "Archivos" (línea 31).
- Poblar `#tab-paquetes` (línea 111): `row` con 2 `card-custom` — izquierda `#paquetesTable`, derecha `#citasPaqueteTable`.
- DataTables (Razor C# 5, sin `?.`/`$""`, BOM UTF-8):
  - `#paquetesTable`: `columns` `Id`(visible:false), `PaqueteNombre`; `ajax` → `ConsultarPaquetesPorPaciente`, `d.pacienteId = pacienteId`.
  - `#citasPaqueteTable`: `columns` `Id`(visible:false), `FechaCita`(render `formatFechaHoraAmPm`), `TratamientoNombre`, `Comentarios`; `ajax` → `ConsultarCitasPorPacientePaquete`, `d.pacientePaqueteId = pacientePaqueteIdSeleccionado` (variable inicial `0`).
  - `paquetesTable.on('click', 'tbody tr', ...)` → `pacientePaqueteIdSeleccionado = paquetesTable.row(this).data().Id; citasPaqueteTable.ajax.reload();`.
  - `shown.bs.tab` → `columns.adjust()` (ya existente, cubre ambos grids al ser visibles).

## Verificación

- **Build**: MSBuild Debug vía VsDevCmd + `aspnet_compiler` (exit 0) para las 3 soluciones/proyectos.
- **BD**: ejecutar `sql/sp_nh_PacientePaquete.sql` en productiva (idempotente). Validar:
  - `EXEC sp_nh_GetPaquetesPorPaciente @PacienteId=248` → 1 fila (`PaqueteNombre='PAQUETE CBX CICRATIZ'`, `Id=9`).
  - `EXEC sp_nh_GetCitasPorPacientePaquete @PacientePaqueteId=9` → 7 filas (citas 526–532).
- **UI**: Tab 4 visible; grid izquierdo lista el paquete; click → grid derecho lista 7 citas; ambos endpoints `ModelResponse<T>` HTTP 200 español.

## Riesgos / Preguntas abiertas

- Spec paralela usa `PacientePaqueteDTO`/`CitaPaqueteDTO` y dice "400" en ids inválidos → reconciliar nombres (`Lectura`) y status (HTTP 200 + IsSuccess=false) en `sdd-archive`.
- FKs bugueadas de `CitaPaquete` irrelevantes (solo lectura, JOINs no dependen de FK).
- SP no ejecutado en BD → grid vacío; mitigado por script versionado + aprobación explícita.
