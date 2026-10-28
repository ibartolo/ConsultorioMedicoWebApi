# Tasks: Paquetes del Expediente — Solo Lectura (Tab 4)

## Phase 1: SQL

- [x] 1.1 Crear `sql/sp_nh_PacientePaquete.sql` (BOM UTF-8) con `sp_nh_GetPaquetesPorPaciente(@PacienteId INT)` usando patrón guard `IF OBJECT_ID(...) IS NULL` + `EXEC('CREATE PROCEDURE ...')` (sin DROP/ALTER): `PacientePaquete PP INNER JOIN Paquete P ON P.Id=PP.PaqueteID WHERE PP.PacienteID=@PacienteId AND P.Estatus=1` → columnas `Id, PacienteID, PaqueteID, PaqueteNombre`. Verificar: sin DROP/ALTER; columnas = DTO.
- [x] 1.2 En el mismo archivo, crear `sp_nh_GetCitasPorPacientePaquete(@PacientePaqueteId INT)`: `CitaPaquete CP INNER JOIN Cita C ON C.Id=CP.CitaID INNER JOIN PacientePaquete PP ON PP.Id=CP.PacientePaqueteID LEFT JOIN Tratamiento T ON T.Id=C.TratamientoID WHERE C.Estatus=1 AND PP.Id=@PacientePaqueteId ORDER BY C.FechaCita DESC` → `Id, FechaCita, TratamientoNombre, Comentarios, CitaPaqueteID, PacientePaqueteID`. Verificar: LEFT JOIN conserva citas sin tratamiento; ORDER BY DESC.

## Phase 2: Entities

- [x] 2.1 Crear `ConsultorioMedicoEntities/Catalogos/PacientePaqueteLecturaDTO.cs`: `int Id, int PacienteID, int PaqueteID, string PaqueteNombre`. Verificar: namespace `ConsultorioMedicoEntities.Catalogos`.
- [x] 2.2 Crear `ConsultorioMedicoEntities/Catalogos/CitaPaqueteLecturaDTO.cs`: `int Id, DateTime? FechaCita, string TratamientoNombre, string Comentarios, int CitaPaqueteID, int PacientePaqueteID`. Verificar: `Id` = Cita.Id.
- [x] 2.3 Registrar ambos en `ConsultorioMedicoEntities.csproj` (`<Compile Include="Catalogos\...LecturaDTO.cs" />`). Verificar: MSBuild Entities exit 0.

## Phase 3: WebApi

- [x] 3.1 `ConsultorioMedicoWebApi/DAL/DbWrapper.Historial.cs`: añadir `ObtenerPaquetesPorPaciente(int, string)` y `ObtenerCitasPorPacientePaquete(int, string)` con `GetObjects` + mapper inline y `SqlParameter("@PacienteId")` / `("@PacientePaqueteId")`. Verificar: FechaCita mapea `Convert.IsDBNull(...) ? (DateTime?)null : Convert.ToDateTime(...)`.
- [x] 3.2 `ConsultorioMedicoWebApi/Services/HistorialService.cs`: añadir ambos métodos con guard `id <= 0` → `ModelResponse IsSuccess=false` (HTTP 200, mensaje español). Verificar: español, patrón `ObtenerTratamientosPorPaciente`.
- [x] 3.3 `ConsultorioMedicoWebApi/Controllers/HistorialController.cs`: añadir `[HttpGet, Route("PaquetesPorPaciente/{pacienteId:int}")]` y `[HttpGet, Route("CitasPorPacientePaquete/{pacientePaqueteId:int}")]` → `_historialService`. Verificar: retorna `ModelResponse<List<...LecturaDTO>>`.

## Phase 4: MVC

- [x] 4.1 `ConsultorioMedicoMVC/DAL/HttpClientConnection.Historial.cs`: añadir `ObtenerPaquetesPorPaciente(int)` / `ObtenerCitasPorPacientePaquete(int)` → `RequestAsync<List<T>>("api/Historial/..." + id, HttpMethod.Get, null, token.Token.access_token)`.
- [x] 4.2 `ConsultorioMedicoMVC/Services/HistorialService.cs`: añadir `ConsultarPaquetesPorPaciente(int)` / `ConsultarCitasPorPacientePaquete(int)` delegando al DAL.
- [x] 4.3 `ConsultorioMedicoMVC/Controllers/HistorialController.cs`: añadir acciones Data Access `ConsultarPaquetesPorPaciente(int pacienteId)` / `ConsultarCitasPorPacientePaquete(int pacientePaqueteId)` → `JsonConvert.SerializeObject(response)`.

## Phase 5: Vista

- [x] 5.1 `ConsultorioMedicoMVC/Views/Historial/Expediente.cshtml`: quitar `d-none` del `<li>` Tab 4 (línea 36); mantener `d-none` en Archivos (línea 31).
- [x] 5.2 Poblar `#tab-paquetes` (línea 111): `row` + 2 `card-custom` con `#paquetesTable` (izq) y `#citasPaqueteTable` (der).
- [x] 5.3 JS: `#paquetesTable` DataTable — columns `Id`(visible:false) + `PaqueteNombre`; ajax `ConsultarPaquetesPorPaciente`, `d.pacienteId = pacienteId`.
- [x] 5.4 JS: `#citasPaqueteTable` DataTable — columns `Id`(visible:false), `FechaCita`(render `formatFechaHoraAmPm`), `TratamientoNombre`, `Comentarios`; ajax `ConsultarCitasPorPacientePaquete`, `d.pacientePaqueteId = pacientePaqueteIdSeleccionado` (var inicial `0`).
- [x] 5.5 JS: `paquetesTable.on('click','tbody tr')` → `pacientePaqueteIdSeleccionado = paquetesTable.row(this).data().Id; citasPaqueteTable.ajax.reload();`. Verificar: `shown.bs.tab` ya hace `columns.adjust()`; Razor C# 5 (sin `?.`/`$""`); BOM UTF-8.

## Phase 6: Verificación

- [x] 6.1 Build: MSBuild Debug (VsDevCmd) + `aspnet_compiler` → exit 0 (3 proyectos).
- [x] 6.2 Ejecutar `sql/sp_nh_PacientePaquete.sql` en BD productiva. Verificar `EXEC sp_nh_GetPaquetesPorPaciente @PacienteId=248` → 1 fila (`PaqueteNombre='PAQUETE CBX CICRATIZ'`, `Id=9`); `EXEC sp_nh_GetCitasPorPacientePaquete @PacientePaqueteId=9` → 7 filas (Ids 526–532).
- [ ] 6.3 Verificar endpoints `ModelResponse<T>` HTTP 200 español y Tab 4 visible con ambos grids (paciente 248; click → 7 citas).
