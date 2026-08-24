# Proposal: Dominio Paciente

## Intent

Modernizar el consultorio médico: de WinForms + ADO.NET (código fuente perdido) a una API Web .NET 10 mantenible. Fase 1: solo el dominio Paciente, ADO.NET estricto (sin ORM). Fuente de verdad: `espesificaiones.md`, `espesificaciones2.md`, `espesificaciones3.md`; **`espesificaciones3.md` prevalece en conflictos**.

## Scope

### In Scope
- Solución `DESI.ConsultorioMedico.sln` (Paciente.* + Common.* + WebApi).
- Entidad Paciente (Factory Method + Fluent Interface) y ValueObjects.
- 3 SPs: `GetAllPacientes`, `GetPacienteById`, `SaveOrUpdatePaciente`.
- 3 endpoints: `GET /api/v1/paciente`, `GET /api/v1/paciente/{id}`, `POST /api/v1/paciente/guardar`.
- `ModelResponse<T>`, DataAnnotations, errores en Controller.
- Serilog, Scalar `/scalar/v1`, CORS abierto, rate limiting 100 req/min.
- `SaveOrUpdatePaciente`: solo campos Request; el SP calcula Edad y auditoría.

### Out of Scope
- Dominios Usuario, Cita, Consulta, Tratamiento, Anexo.
- SPs de filtrado: `GetPacientesFiltrados`, `GetAllPacienteByEstatus`, `GetPacienteByFechaNacimiento`.
- JWT (dominio Usuario), paginación, borrado físico, frontend MVC.

## Capabilities

### New Capabilities
- `paciente`: entidad, DTOs, contratos de repositorio, servicios, SPs y endpoints.

### Modified Capabilities
- None (greenfield; `openspec/specs/` vacío).

## Approach

Capas por dominio + Common.* + WebApi. Flujo: Controller → Aplicacion → Infraestructura (ADO.NET → SP) → Mapp → entidad → `ModelResponse<T>`. Se crea la estructura .NET 10 y luego el dominio Paciente (Fluent Interface, Factory Method, Repository, DI, DTO, Static Mapper). Common.* y WebApi son soporte de diseño, no de especificación.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `DESI.ConsultorioMedico.sln` | New | solución y carpetas |
| `Paciente.{Dominio,Aplicacion,Infraestructura,Mensajes,Pruebas}` | New | 5 proyectos por capa |
| `Common.*` (Data/Http/Helpers/Mensajes) | New | BaseRepository, OperationResult, helpers |
| `ConsultorioMedicoWebApi` | New | PacienteController, Program.cs, config |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Firma del SP vs BD real | Med | Validar contra SPs reales al implementar |
| Greenfield sin código de referencia | Med | Especificación como única fuente de verdad |
| Mapeo ADO.NET manual | Med | Extensiones SqlDataReader + pruebas por flujo |
| Credenciales en appsettings | Low | No commitear secretos reales |

## Rollback Plan

Greenfield: sin código previo que revertir. Si falla, descartar la rama (`git reset --hard` / `git branch -D`); especificaciones intactas. Los SPs no se alteran sin script versionado (revertible manualmente).

## Dependencies

- SQL Server 2019+ (`dbo.Paciente` + 3 SPs).
- .NET 10 SDK + Visual Studio 2026.
- NuGet: Serilog, Scalar.AspNetCore, xUnit.

## Success Criteria

- [ ] `dotnet build` compila sin errores.
- [ ] Los 3 endpoints responden `ModelResponse<T>`.
- [ ] `GET /api/v1/paciente/{id}` retorna 404 controlado; `POST guardar` retorna el Id.
- [ ] Scalar en `/scalar/v1`.
- [ ] Pruebas xUnit del dominio Paciente en verde.
