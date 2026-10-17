# Proposal: Paquetes del Paciente (Tab 4 del Expediente)

## Intent

Implementar la Tab 4 "Paquetes" del expediente (`Views/Historial/Expediente.cshtml`), hoy oculta (`d-none`), replicando la pestaña Paquetes del `CatalogoHistorial` WinForms: paquetes adquiridos por el paciente (`PacientePaquete`), pagos (`PagoPaquete`) y tratamientos tomados del paquete (`CitaPaquete`). Las 4 tablas puente existen en `script.sql` pero hoy tienen **cero referencias C#** (solo existe el CRUD del catálogo `Paquete`).

## Scope

### In Scope
- Entidades/DTO/validadores nuevos: `PacientePaquete`, `PagoPaquete`, `CitaPaquete` (+ DTOs de lectura) en `ConsultorioMedicoEntities/Catalogos/`.
- SPs nuevos `sp_nh_*` (solo `CREATE` + `IF OBJECT_ID`): `sp_nh_GetPacientePaqueteByPacienteId`, `sp_nh_SavePacientePaquete`, `sp_nh_SavePagoPaquete`, `sp_nh_GetCitaPaqueteByPacientePaqueteId`, `sp_nh_SaveCitaPaquete`.
- Reutilizar SPs existentes: `GetAllPagoPaqueteByPacientePaqueteIdDT` (Precio/Pagado/Adeuda) y `GetAllTratamiendoByPaqueteID` (tratamientos del paquete) — solo falta su wrapper C#.
- WebApi: `DbWrapper.PacientePaquete.cs` + `PacientePaqueteService.cs` + `PacientePaqueteController.cs` (`[Authorize] [RoutePrefix("api/PacientePaquete")]`).
- MVC: `HttpClientConnection.PacientePaquete.cs` + `PacientePaqueteService.cs` + acciones AJAX en `HistorialController`.
- UI Tab 4 en `Expediente.cshtml`: quitar `d-none`; 2 secciones (izq. grid paquetes + "nuevo paquete"; der. detalle con tratamientos del paquete, comentarios, "Nuevo Pago", etiquetas Precio/Pagado/Adeuda, grid inferior de tratamientos tomados). Paciente en contexto (`pacienteId`).
- Reutilizar `api/Paquete/List` (combo catálogo) y `api/Tratamiento/List` (combos tratamiento).

### Out of Scope
- Catálogo `Paquete` global y su CRUD (ya existe).
- Tabs 1/2/3 del expediente y `Historial/Index`.
- Composición de `TratamientoPaquete` (se administra en el catálogo Paquete; acá solo lectura — NO requiere save nuevo).
- Corrección de las FKs bugueadas de `CitaPaquete` en BD (NO DROP/ALTER en productiva).

## Capabilities

### New Capabilities
- `paquetes-paciente`: capa `PacientePaquete`/`PagoPaquete`/`CitaPaquete` + endpoints + Tab 4 del expediente.

### Modified Capabilities
- `expediente-clinico`: Tab 4 "Paquetes" deja de estar oculta (`d-none`) y pasa a ser funcional.

## Approach

Flujo MVC→WebApi→SP existente. Sección izquierda = grid `PacientePaquete` (join Paquete → Id, Nombre, PacienteID, PacientePaqueteID) + modal "nuevo paquete" que asocia un `Paquete` del catálogo al paciente (`sp_nh_SavePacientePaquete`). Sección derecha (requiere `PacientePaqueteID`): nombre del paquete + tratamientos del paquete (catálogo, `GetAllTratamiendoByPaqueteID`), etiquetas Precio/Pagado/Adeuda (`GetAllPagoPaqueteByPacientePaqueteIdDT`), "Nuevo Pago" (`sp_nh_SavePagoPaquete`) y grid de tratamientos tomados (`CitaPaquete`). "Guardar Tratamiento" crea **Cita + CitaPaquete juntos** (`sp_nh_SaveCita` ya existe + `sp_nh_SaveCitaPaquete` nuevo), fiel a la WinForms (nunca un Tratamiento suelto). `TratamientoPaquete` queda solo lectura (wrapper del SP existente, sin save).

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `ConsultorioMedicoEntities/Catalogos/PacientePaquete.cs` | New | POCO {Id, PacienteID, PaqueteID} |
| `ConsultorioMedicoEntities/Catalogos/PacientePaqueteDTO.cs` | New | grid: {Id, Nombre, PacienteID, PacientePaqueteID} |
| `ConsultorioMedicoEntities/Catalogos/PagoPaquete.cs` / `PagoPaqueteDTO.cs` | New | POCO {Id, Pago, FechaPago, PacientePaqueteID} + DTO {Precio, Pagado, Adeuda} |
| `ConsultorioMedicoEntities/Catalogos/CitaPaquete.cs` / `CitaPaqueteDTO.cs` | New | POCO {Id, CitaID, PacientePaqueteID} + DTO grid {Cita + TratamientoNombre} |
| `ConsultorioMedicoEntities/Catalogos/TratamientoPaqueteDTO.cs` | New | {Id, Nombre} (lista de tratamientos del paquete) |
| `ConsultorioMedicoEntities/Catalogos/*Validator.cs` | New | validadores estáticos PacientePaquete/PagoPaquete |
| `ConsultorioMedicoEntities/ConsultorioMedicoEntities.csproj` | Modified | registrar `.cs` nuevos |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.PacientePaquete.cs` | New | ObtenerPorPaciente / Guardar / GuardarPago / CitasPorPacientePaquete / PagoResumen / GuardarCitaPaquete |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.Paquete.cs` | Modified | `ObtenerTratamientosPorPaquete` → `GetAllTratamiendoByPaqueteID` |
| `ConsultorioMedicoWebApi/Services/PacientePaqueteService.cs` | New | validación + orquestación |
| `ConsultorioMedicoWebApi/Services/PaqueteService.cs` | Modified | tratamientos por paquete |
| `ConsultorioMedicoWebApi/Controllers/PacientePaqueteController.cs` | New | `[Authorize] [RoutePrefix("api/PacientePaquete")]` |
| `ConsultorioMedicoWebApi/Controllers/PaqueteController.cs` | Modified | `GET Tratamientos/{paqueteId}` |
| `ConsultorioMedicoMVC/DAL/HttpClientConnection.PacientePaquete.cs` | New | llamadas a `api/PacientePaquete/*` |
| `ConsultorioMedicoMVC/DAL/HttpClientConnection.Paquete.cs` | Modified | tratamientos por paquete |
| `ConsultorioMedicoMVC/Services/PacientePaqueteService.cs` | New | servicio MVC |
| `ConsultorioMedicoMVC/Controllers/HistorialController.cs` | Modified | acciones AJAX (consultar/guardar paquetes, pagos, citas-in-paquete) |
| `ConsultorioMedicoMVC/Views/Historial/Expediente.cshtml` | Modified | quitar `d-none` Tab 4 + markup/JS 2 secciones (Razor C# 5, BOM UTF-8) |
| `sql/sp_nh_PacientePaquete.sql` | New | SPs nuevos (solo `CREATE` + `IF OBJECT_ID`) |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| **FKs bugueadas de `CitaPaquete`** (`FK_CitaPaquete_Cita`→`Paciente(Id)`, `FK_CitaPaquete_PacientePaquete`→`Paquete(Id)`) hacen fallar el `INSERT` de `sp_nh_SaveCitaPaquete` | High | Verificar en BD productiva (`sys.foreign_keys`) antes de implementar; NO DROP/ALTER; si las FKs existen, diferir/condicionar "Guardar Tratamiento" (decisión en design) |
| `GetAllPagoPaqueteByPacientePaqueteIdDT` usa INNER JOIN `PagoPaquete` → sin pagos devuelve 0 filas (sin Precio/Pagado/Adeuda) | Med | Si no hay filas, calcular Precio desde `Paquete.Costo` + Pagado=0 en C#; o SP `sp_nh_` con LEFT JOIN |
| Columnas nullable en las 4 tablas (`PacienteID`, `PaqueteID`, `Pago`, etc.) | Low | Validadores exigen Ids > 0 / Pago ≥ 0 |
| Vista `Expediente.cshtml` crece mucho (2 secciones + modales) | Med | Sección autocontenida, patrón existente; extraer parcial en change futuro |

## Rollback Plan

Todo aditivo y de solo lectura sobre lo existente (salvo inserts). Revertir con `git checkout` de archivos nuevos/modificados. Los SPs nuevos no se ejecutan en BD productiva salvo aprobación explícita — se versionan solo como script (`sql/*.sql`), sin DROP/ALTER. Sin migración de datos ni cambios de esquema.

## Dependencies

- BD `DB_9C7990_Consultorio` con tablas `PacientePaquete`, `PagoPaquete`, `TratamientoPaquete`, `CitaPaquete`, `Paquete`, `Tratamiento`, `Cita` (ya existen en `script.sql`).
- SPs existentes: `GetAllPagoPaqueteByPacientePaqueteIdDT`, `GetAllTratamiendoByPaqueteID`, `sp_nh_SaveCita`.
- Endpoints existentes: `api/Paquete/List`, `api/Tratamiento/List`.

## Success Criteria

- [ ] MSBuild Debug + `aspnet_compiler` exit 0.
- [ ] Tab 4 visible y funcional con `pacienteId` en contexto.
- [ ] Grid izquierdo lista paquetes adquiridos; "nuevo paquete" asocia vía `sp_nh_SavePacientePaquete`.
- [ ] Detalle muestra Precio/Pagado/Adeuda (`GetAllPagoPaqueteByPacientePaqueteIdDT`) y "Nuevo Pago" inserta `PagoPaquete`.
- [ ] Grid inferior lista tratamientos tomados (`sp_nh_GetCitaPaqueteByPacientePaqueteId`) y "Guardar Tratamiento" crea Cita + CitaPaquete (sujeto a verificación de FKs).
- [ ] Todos los endpoints responden `ModelResponse<T>` (HTTP 200, español).

## Notas

- **Bug heredado `CitaPaquete`** (verificado en `script.sql` líneas 384–392): `FK_CitaPaquete_Cita` referencia `Paciente(Id)` y `FK_CitaPaquete_PacientePaquete` referencia `Paquete(Id)`. La WinForms escribía por NHibernate (mapeo de entidades, no constraints); en web el `INSERT` directo puede fallar. Decisión de diseño obligatoria antes de implementar `sp_nh_SaveCitaPaquete` (ver Riesgos).
- **`GetAllPagoPaqueteByPacientePaqueteIdDT`** solo devuelve fila si ya existe ≥1 `PagoPaquete` (INNER JOIN) — manejar el caso "sin pagos".
- **`TratamientoPaquete` NO requiere save nuevo**: la composición paquete↔tratamiento se administra en el catálogo Paquete; acá es solo lectura (`GetAllTratamiendoByPaqueteID`).
