# Verification Report — dominio-paciente

**Change**: dominio-paciente
**Fecha**: 2026-08-24
**Modo**: Standard (strict_tdd = false)
**Versión spec**: 9 requirements, 19 scenarios (el prompt del orquestador indicaba 15; el spec real contiene 19)

---

## Resumen ejecutivo

La implementación del dominio Paciente está **completa y funcionalmente alineada** con el spec y el design. El build compila con **0 errores / 0 warnings** y las **26 pruebas xUnit pasan en verde**. La arquitectura por capas, el acceso ADO.NET estricto (sin ORM), el contrato `IPacienteRepository` en Infraestructura, la firma de 10 parámetros de `SaveOrUpdatePaciente`, `Sexo` como string (no enum), el sobre `ModelResponse<T>`, el Factory Method + Fluent Interface y el `Mapp` estático están todos implementados conforme a las decisiones de diseño.

Se detectan **0 hallazgos CRITICAL**, **4 WARNING** y **3 SUGGESTION**. Ninguno bloquea el archive; los WARNING son riesgos/desviaciones menores (la más relevante: la validación de `Sexo` "Masculino"/"Femenino" no se aplica a nivel de Request en el endpoint POST, y la firma RETURN vs OUTPUT del SP sigue sin confirmarse contra BD real — pregunta abierta del propio design).

---

## Completeness (Tareas)

| Métrica | Valor |
|---------|-------|
| Tareas totales | 33 (1.1–9.3) |
| Completadas `[x]` | 32 |
| Incompletas `[ ]` | 1 |

Tarea incompleta: **9.3 Smoke test manual (opcional, requiere BD)** — explícitamente opcional y dependiente de base de datos real; fuera del alcance de la verificación unitaria.

---

## Build & Tests (Ejecución real)

**Build**: ✅ Passed
```
Compilación correcta.
    0 Advertencia(s)
    0 Errores
```
Comando: `dotnet build DESI.ConsultorioMedico.sln`

**Tests**: ✅ 26 passed / ❌ 0 failed / ⚠️ 0 skipped
```
Correctas! - Con error: 0, Superado: 26, Omitido: 0, Total: 26, Duración: 82 ms - Paciente.Pruebas.dll (net10.0)
```
Comando: `dotnet test Paciente/Paciente.Pruebas/Paciente.Pruebas.csproj`

**Coverage**: ➖ No disponible (config `coverage.available: false`; sin umbral configurado).

---

## Spec Compliance Matrix

| # | Requirement | Scenario | Test | Resultado |
|---|-------------|----------|------|-----------|
| 1 | Entidad Paciente | Construcción válida con Fluent Interface | `PacienteTests > Set_Fluent_RetornaMismaInstancia`, `Set_AsignaValoresCorrectos` | ✅ COMPLIANT |
| 2 | Entidad Paciente | Validación en el método Set | `PacienteTests > SetNombre_Vacio_LanzaArgumentException` | ✅ COMPLIANT |
| 3 | ValueObjects | Email inválido | `ValueObjectsTests > Email_FormatoInvalido_LanzaArgumentException` | ✅ COMPLIANT |
| 4 | ValueObjects | Sexo fuera de catálogo | `ValueObjectsTests > Sexo_ValorNoPermitido_LanzaArgumentException` | ✅ COMPLIANT |
| 5 | ModelResponse\<T\> | Respuesta exitosa | `PacienteAplicacionTests > ObtenerTodos_ConDatos_RetornaExito` | ✅ COMPLIANT |
| 6 | ModelResponse\<T\> | Respuesta de error controlado | `PacienteAplicacionTests > ObtenerPorId_Inexistente_RetornaFallo` | ✅ COMPLIANT |
| 7 | DTO GuardarPacienteRequest | Request válido | (ninguno) | ❌ UNTESTED |
| 8 | DTO GuardarPacienteRequest | Request sin Nombre | (ninguno) | ❌ UNTESTED |
| 9 | DTOs PacienteResponse/GetPacientesResponse | NombreCompleto compuesto | `MappTests > MapToPacienteResponse_ComponeNombreCompleto` | ✅ COMPLIANT |
| 10 | GET /api/v1/paciente | Lista con pacientes | `PacienteAplicacionTests > ObtenerTodos_ConDatos_RetornaExito` | ✅ COMPLIANT |
| 11 | GET /api/v1/paciente | Lista vacía | `PacienteAplicacionTests > ObtenerTodos_SinDatos_RetornaListaVacia` | ✅ COMPLIANT |
| 12 | GET /api/v1/paciente/{id} | Paciente encontrado | `PacienteAplicacionTests > ObtenerPorId_Existente_RetornaPaciente` | ✅ COMPLIANT |
| 13 | GET /api/v1/paciente/{id} | Paciente no encontrado | `PacienteAplicacionTests > ObtenerPorId_Inexistente_RetornaFallo` | ✅ COMPLIANT |
| 14 | GET /api/v1/paciente/{id} | Id inválido | (ninguno directo) | ⚠️ PARTIAL |
| 15 | POST guardar | Crear paciente | `PacienteAplicacionTests > Guardar_IdCero_CreaPaciente` | ✅ COMPLIANT |
| 16 | POST guardar | Actualizar paciente | `PacienteAplicacionTests > Guardar_IdMayorCero_ActualizaPaciente` | ✅ COMPLIANT |
| 17 | POST guardar | Validación fallida | (ninguno directo) | ⚠️ PARTIAL |
| 18 | Flujo de datos | Flujo de obtención | (estructural + tests de Aplicacion) | ✅ COMPLIANT |
| 19 | Flujo de datos | Flujo de guardado | (estructural + tests de Aplicacion) | ✅ COMPLIANT |

**Resumen de cumplimiento**: 15/19 escenarios COMPLIANT, 2 PARTIAL, 2 UNTESTED.

---

## Correctness (Static — Structural Evidence)

| Requirement | Estado | Notas |
|-------------|--------|-------|
| Entidad Paciente (Factory + Fluent + solo lectura) | ✅ Implementado | ctor privado + `Create()`; `Set…` retornan `this`; propiedades `private set`; validación con `ArgumentException`. |
| ValueObjects Email/Telefono/Sexo | ✅ Implementado | `Email` (regex + máx 50), `Telefono` (máx 100), `Sexo` (string, máx 15, solo M/F). |
| ModelResponse\<T\> | ✅ Implementado | `{ IsSuccess, Message, Response }` + `Success`/`Failure`. `Response` es `T?` (mejora para caso de error). |
| DTO GuardarPacienteRequest (10 campos + DataAnnotations) | ✅ Implementado | `Nombre`/`ApellidoPaterno` `[Required][MaxLength(50)]`; `Email` `[EmailAddress][MaxLength(50)]`; `TelefonoCel` `[MaxLength(100)]`; `Comentarios` `[MaxLength(1000)]`; `Sexo` `[MaxLength(15)]`; `Estatus` `[Required]`. Sin Edad/FechaAlta/auditoría. |
| DTOs PacienteResponse / GetPacientesResponse | ✅ Implementado | 12 campos + `NombreCompleto`; `GetPacientesResponse` solo expone `Pacientes`. |
| GET /api/v1/paciente | ✅ Implementado | `ObtenerTodos()` → `GetAllPacientes` → `Mapp` → `ModelResponse<GetPacientesResponse>`; 200 con lista y con vacío. |
| GET /api/v1/paciente/{id} | ⚠️ Parcial | 400 si `id<=0` ✓; 404 si no encontrado ✓; **id no numérico → 404 por constraint `{id:int}` en vez de 400** (ver WARNING-2). |
| POST /api/v1/paciente/guardar | ⚠️ Parcial | 400 en `ModelState` inválido ✓; 200 con Id ✓; **`Sexo` inválido no produce 400** (ver WARNING-1). |
| Flujo de datos por endpoint | ✅ Implementado | Controller → Aplicacion → Infraestructura (ADO.NET/SP) → Mapp → entidad/DTO → ModelResponse; errores en Controller. |

---

## Coherence (Design — ADR)

| Decisión | ¿Cumplida? | Notas |
|----------|-----------|-------|
| ADR-1 Capas por dominio (5 proyectos) + Common + WebApi | ✅ Sí | 10 proyectos creados; namespaces `DESI.ConsultorioMedico.*`. |
| ADR-2 ADO.NET estricto (sin ORM) | ✅ Sí | `SqlConnection`/`SqlCommand`/`SqlDataReader` vía `BaseRepository`. Sin EF/Dapper. |
| ADR-3 Firma `SaveOrUpdatePaciente` = 10 params | ✅ Sí | Exactamente `@Id,@Nombre,@ApellidoPaterno,@ApellidoMaterno,@Email,@TelefonoCel,@Comentarios,@Sexo,@FechaNacimiento,@Estatus`. No envía Edad/FechaAlta/auditoría. |
| ADR-4 Sexo string con VO (no enum) | ✅ Sí | `Sexo` VO con constantes "Masculino"/"Femenino". |
| ADR-5 Envelope `ModelResponse<T>` / `OperationResult` interno | ✅ Sí | `ModelResponse` público; `OperationResult<int>` entre Infra/Aplicacion; `GetPacientesResponse` no embebe OperationResult. |
| ADR-6 `IPacienteRepository` en Infraestructura | ✅ Sí | Vive en `Paciente.Infraestructura` (NO en Aplicacion ni Dominio). `Aplicacion → Infraestructura`. |
| ADR-7 Dependencias (Infra → Mensajes → Dominio) | ✅ Sí | `PacienteData` (crudo) en Mensajes; `Mapp` mapea sin referenciar Infraestructura. Sin ciclo. |
| ADR-8 Orden de `GetAllPacientes` lo define el SP | ✅ Sí | Sin `ORDER BY` en código. |
| ADR-9 Manejo de errores a nivel Controller | ✅ Sí | Sin middleware global de errores. |
| ADR-10 `Entity<T>` en `Paciente.Dominio` | ✅ Sí | `Entity<int>` con `Id` getter protegido. |

**Referencias de proyecto**: verificadas y consistentes con `design.md` §Estructura (Dominio sin refs; Mensajes→Dominio; Infra→{Dominio,Mensajes,Common.Data,Common.Mensajes}; Aplicacion→{Infra,Dominio,Mensajes,Common.Mensajes}; Pruebas→{Dominio,Aplicacion,Infra,Mensajes,Common.Mensajes}; WebApi→todo). `Common.Data`→`Microsoft.Data.SqlClient 7.0.2`.

---

## Issues Found

### CRITICAL (deben corregirse antes del archive)
Ninguno.

### WARNING (deberían corregirse, no bloquean)

- **WARNING-1 — `Sexo` no se valida "Masculino"/"Femenino" a nivel de Request (POST)**.
  - `GuardarPacienteRequest.Sexo` solo declara `[MaxLength(15)]`. La validación de catálogo vive en el VO `Sexo` (Dominio), que **no se ejercita** en el flujo de guardado (el design indica que el guardado no hace ida y vuelta por entidad). Por tanto, un request con `Sexo="Otro"` pasa `ModelState.IsValid` y **no** produce 400, incumpliendo el escenario "Validación fallida" (`THEN responde 400 y no invoca el SP`). Los casos `Nombre` vacío y `Email` inválido sí producen 400.
  - Archivo: `Paciente/Paciente.Mensajes/GuardarPacienteRequest.cs:34-35`; `ConsultorioMedicoWebApi/Controllers/PacienteController.cs:70-84`.
  - Nota: es una tensión spec↔design (ADR-4 valida Sexo en el VO; el design no añade validación de catálogo en el Request). Recomendación: añadir validación de catálogo en el Request (p. ej. atributo propio o check en el Controller) si el escenario 400 con Sexo inválido es obligatorio.

- **WARNING-2 — `GET /api/v1/paciente/{id}` con id no numérico devuelve 404 en vez de 400**.
  - La ruta usa `[HttpGet("{id:int}")]`: un `id` no numérico (ej. `abc`) no coincide con la ruta y el framework retorna 404, mientras el escenario "Id inválido" especifica `THEN responde 400 por validación` para `id no numérico o ≤ 0`. El caso `id ≤ 0` sí devuelve 400 correctamente.
  - Archivo: `ConsultorioMedicoWebApi/Controllers/PacienteController.cs:41,47-50`.

- **WARNING-3 — Mensajes hardcodeados en el Controller** (regla del design "nunca hardcodeados en el Controller").
  - `"El identificador debe ser mayor que cero."` y el fallback `"Datos de entrada inválidos."` están hardcodeados en `PacienteController`, en lugar de provenir de `SystemMessage`. No son los 4 mensajes del catálogo, pero la regla se declara de forma absoluta en `design.md` (línea 235).
  - Archivo: `ConsultorioMedicoWebApi/Controllers/PacienteController.cs:49,77`.

- **WARNING-4 — Firma de retorno del SP `SaveOrUpdatePaciente` (RETURN vs OUTPUT) sin confirmar contra BD real**.
  - `BaseRepository.ExecuteNonQueryAsync` lee `@ReturnValue` con `ParameterDirection.ReturnValue` exclusivamente. Si el SP real devuelve el Id vía parámetro `OUTPUT` (y no `RETURN`), no se obtendría el Id. Es la "Pregunta Abierta" nº1 del propio `design.md` y no puede confirmarse sin la BD real (escenario 9.3, opcional, pendiente).
  - Archivo: `Common/Common.Data/BaseRepository.cs:87-95`; `Paciente/Paciente.Infraestructura/PacienteInfraestructura.cs:55`.

### SUGGESTION (mejoras opcionales)

- **SUGGESTION-1 — Falta test unitario de DataAnnotations del DTO** (`GuardarPacienteRequest`). Los escenarios "Request válido" y "Request sin Nombre" no tienen test automatizado (la validación está declarada correctamente en código, pero no se demuestra en runtime). Añadir tests que validen `ModelState`/`Validator.TryValidateObject` cubriría los escenarios 7 y 8.
- **SUGGESTION-2 — `[Required]` sobre `Estatus` (bool) es un no-op** en DataAnnotations (un `bool` no anulable siempre tiene valor). Coincide literalmente con el spec, pero no aporta validación real; si se desea validar presencia, usar `bool?` + `[Required]`.
- **SUGGESTION-3 — `ObtenerPorId` reutiliza el mensaje "Pacientes obtenidos correctamente"** (plural) para el caso singular "encontrado". Funcional (el spec no exige mensaje específico para "encontrado"), pero un mensaje singular sería más preciso.

---

## Verdict

**PASS WITH WARNINGS**

La implementación está completa, compila sin errores, pasa las 26 pruebas y respeta la arquitectura y las decisiones de diseño. El cambio está **listo para archive**; los 4 WARNING no bloquean y el más relevante (validación de `Sexo` en el Request) es una tensión spec↔design que el orquestador puede resolver como follow-up o aceptar como alcance de Fase 1.
