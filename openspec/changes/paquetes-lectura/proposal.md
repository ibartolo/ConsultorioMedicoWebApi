# Proposal: Paquetes del Expediente — Solo Lectura (Tab 4)

## Intent

Habilitar la Tab 4 "Paquetes" del Expediente Clínico (`Historial/Expediente/{id}`), hoy oculta (`d-none`) y vacía, replicando **en solo lectura** la pestaña Paquetes del `CatalogoHistorial` WinForms: listar los paquetes adquiridos por el paciente y, al seleccionar uno, las citas-tratamientos de ese paquete. Sin tocar la estructura de BD, sin pagos, sin altas.

## Scope

### In Scope
- Quitar `d-none` del `<li>` Tab 4 y poblar el panel `#tab-paquetes` (hoy vacío, línea 111).
- Grid izquierdo: `PacientePaquete` JOIN `Paquete` → columna visible **Nombre**.
- Grid derecho: al seleccionar paquete, `CitaPaquete` JOIN `Cita` JOIN `Tratamiento` → **FechaCita, Tratamiento, Comentarios**.
- 2 SPs nuevos `sp_nh_*` (solo `CREATE` + `IF OBJECT_ID`, sin DROP/ALTER).
- 2 DTOs, 2 endpoints WebApi GET, 2 llamadas MVC, 2 acciones AJAX, 2 DataTables AJAX.

### Out of Scope
- **BD estructural**: NO se corrigen las FKs bugueadas de `CitaPaquete`; NO se alteran tablas/FKs/objetos existentes.
- NO pagos (`PagoPaquete`), NO altas (asociar paquete), NO guardar tratamiento del paquete.
- `Historial/Index`, `Cita/Index`, `Consulta/Index`, Tabs 1/2/3 del expediente.
- El change full-scope `paquetes-del-paciente` (altas/pagos/guardar) queda a futuro.

## Capabilities

### New Capabilities
- `paquetes-lectura`: lectura de paquetes adquiridos + citas-tratamientos del paquete (grids solo lectura Tab 4).

### Modified Capabilities
- `expediente-clinico`: Tab 4 deja de estar oculta y muestra 2 grids de solo lectura.

## Approach

Solo lectura, flujo MVC→WebApi→SP (patrón existente `sp_nh_GetTratamientosPorPaciente`):

1. **SQL — archivo nuevo `sql/sp_nh_PacientePaquete.sql`.** Justificación: `sp_nh_Historial.sql` cubre historia Cita/Consulta y `sp_nh_Paquete.sql` cubre CRUD del catálogo Paquete; un archivo dedicado agrupa el dominio puente y coincide con el archivo que ya prevé `paquetes-del-paciente`.
   - `sp_nh_GetPaquetesPorPaciente(@PacienteId)`: `PacientePaquete pp JOIN Paquete p ON pp.PaqueteID=p.Id` → `Id, PacienteID, PaqueteID, PaqueteNombre (p.Nombre)`.
   - `sp_nh_GetCitasPorPacientePaquete(@PacientePaqueteId)`: `CitaPaquete cp JOIN Cita c ON cp.CitaID=c.Id LEFT JOIN Tratamiento t ON c.TratamientoID=t.Id` → `Id, FechaCita, TratamientoNombre, Comentarios, CitaPaqueteID, PacientePaqueteID`. (LEFT JOIN en Tratamiento para no perder citas sin tratamiento.)
2. **Entities**: `PacientePaqueteDTO` {Id, PacienteID, PaqueteID, PaqueteNombre} y `CitaPaqueteDTO` {Id, FechaCita, TratamientoNombre, Comentarios, CitaPaqueteID, PacientePaqueteID}; registrar en csproj.
3. **WebApi**: `DbWrapper.Historial` + `HistorialService` + `HistorialController` → `GET api/Historial/PaquetesPorPaciente/{pacienteId}` y `GET api/Historial/CitasPorPacientePaquete/{pacientePaqueteId}`.
4. **MVC**: `HttpClientConnection.Historial.cs` + `Services/HistorialService.cs` + acciones AJAX `ConsultarPaquetesPorPaciente` / `ConsultarCitasPorPacientePaquete` en `HistorialController`.
5. **Vista**: Tab 4 → grid izquierdo `#paquetesTable` (Nombre); grid derecho `#citasPaqueteTable` (FechaCita, Tratamiento, Comentarios) que se recarga al hacer **click** en una fila de paquetes (selección de fila; patrón análogo al `dblclick` existente en `consultasTable`).

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `sql/sp_nh_PacientePaquete.sql` | New | 2 SPs SELECT (solo CREATE + IF OBJECT_ID) |
| `ConsultorioMedicoEntities/Catalogos/PacientePaqueteDTO.cs` | New | {Id, PacienteID, PaqueteID, PaqueteNombre} |
| `ConsultorioMedicoEntities/Catalogos/CitaPaqueteDTO.cs` | New | {Id, FechaCita, TratamientoNombre, Comentarios, CitaPaqueteID, PacientePaqueteID} |
| `ConsultorioMedicoEntities/ConsultorioMedicoEntities.csproj` | Modified | registrar 2 `.cs` |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.Historial.cs` | Modified | 2 métodos de lectura |
| `ConsultorioMedicoWebApi/Services/HistorialService.cs` | Modified | 2 métodos + validación `>0` |
| `ConsultorioMedicoWebApi/Controllers/HistorialController.cs` | Modified | 2 `[HttpGet]` |
| `ConsultorioMedicoMVC/DAL/HttpClientConnection.Historial.cs` | Modified | 2 llamadas GET |
| `ConsultorioMedicoMVC/Services/HistorialService.cs` | Modified | 2 métodos |
| `ConsultorioMedicoMVC/Controllers/HistorialController.cs` | Modified | 2 acciones AJAX |
| `ConsultorioMedicoMVC/Views/Historial/Expediente.cshtml` | Modified | quitar `d-none` Tab 4 + markup/JS 2 DataTables (Razor C# 5, BOM UTF-8) |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| FKs bugueadas de `CitaPaquete` | Low | Solo lectura (sin INSERT); los JOINs no dependen de FKs |
| Citas sin tratamiento se pierden con INNER JOIN | Med | LEFT JOIN `Tratamiento` |
| SP nuevo no ejecutado en BD → grid vacío | Med | Versionar script; ejecutar en productiva con aprobación explícita |
| Vista `Expediente.cshtml` crece | Low | Sección autocontenida, patrón existente |

## Rollback Plan

Todo aditivo y de solo lectura. Revertir con `git checkout` de archivos nuevos/modificados; re-añadir `d-none` al `<li>` Tab 4 si se desea ocultar. Los SPs son solo `SELECT` y se versionan como script (sin migración de datos ni cambios de esquema); si se ejecutaron, se eliminan con `DROP PROCEDURE` guardado.

## Dependencies

- BD `DB_9C7990_Consultorio`: tablas `PacientePaquete`, `CitaPaquete`, `Paquete`, `Cita`, `Tratamiento` (ya existen).
- Patrones existentes del módulo Historial (`sp_nh_GetTratamientosPorPaciente`, `HttpClientConnection.Historial.cs`).
- Bootstrap 5 + DataTables 1.13.6 (sin moment.js).

## Success Criteria

- [ ] MSBuild Debug + `aspnet_compiler` exit 0.
- [ ] Tab 4 visible; grid izquierdo lista `PAQUETE CBX CICRATIZ` para paciente 248 (`PacientePaquete.Id=9`).
- [ ] Al seleccionarlo, grid derecho lista 7 citas (Ids 526–532).
- [ ] Ambos endpoints responden `ModelResponse<T>` (HTTP 200, español).

## Notas

- **Bug heredado `CitaPaquete`** (verificado): `FK_CitaPaquete_Cita`→`Paciente(Id)`, `FK_CitaPaquete_PacientePaquete`→`Paquete(Id)`. Irrelevante aquí por ser solo lectura; se abordará en `paquetes-del-paciente` (altas/pagos/guardar) a futuro.
