# Tareas: Dominio Paciente

Repositorio greenfield: no existen `.sln`/`.csproj`. La Fase 1 crea la soluciÃ³n y los **10 proyectos** del alcance (5 `Paciente.*` + 4 `Common.*` + `ConsultorioMedicoWebApi`), segÃºn `design.md` y `proposal.md`.

## Decisiones resueltas (para sdd-apply)

- **`IPacienteRepository` vive en `Paciente.Infraestructura`** (contrato + implementaciÃ³n). Infraestructura abstrae cualquier origen de datos (SQL hoy, HTTP futuro) detrÃ¡s del contrato. `Aplicacion` referencia Infraestructura y depende de la abstracciÃ³n vÃ­a DI; no hay ciclo porque Infraestructura no referencia Aplicacion.
- **`Common.Helpers` incremental/on-demand.** Solo se crean los helpers que el cÃ³digo de Fase 1 consuma (validaciÃ³n/fechas/cadenas). **NO** crear `EncryptionHelper` ni helpers de email (dominios futuros).
- **Sin JWT** en Fase 1 (dominio Usuario). CORS abierto, rate limiting 100 req/min, Serilog, Scalar.
- `SaveOrUpdatePaciente` recibe **10 parÃ¡metros** (campos del Request); `Edad`, `FechaAlta` y auditorÃ­a los calcula el SP.
- Todo endpoint retorna `ModelResponse<T>`. Mensajes textuales desde `SystemMessage` (espaÃ±ol).

---

## Fase 1: Infraestructura â€” Scaffolding

- [x] **1.1 Crear la soluciÃ³n `DESI.ConsultorioMedico.sln` y carpetas lÃ³gicas**
  - DescripciÃ³n: `dotnet new sln -n DESI.ConsultorioMedico` en la raÃ­z; agregar carpetas de soluciÃ³n `Paciente/` y `Common/` (`dotnet sln add` con `--solution-folder`).
  - AceptaciÃ³n: `dotnet sln list` muestra la soluciÃ³n; los proyectos quedarÃ¡n agrupados en `Paciente/` y `Common/`.

- [x] **1.2 Crear los 4 proyectos Common (Class Library, `net10.0`)**
  - DescripciÃ³n: `dotnet new classlib` para `Common.Data`, `Common.Http`, `Common.Helpers`, `Common.Mensajes`; namespace `DESI.ConsultorioMedico.Common.*`; eliminar `Class1.cs`; agregarlos a la soluciÃ³n (carpeta `Common/`).
  - AceptaciÃ³n: los 4 proyectos compilan; `Common.Http` queda como esqueleto (sin implementaciÃ³n en Fase 1).

- [x] **1.3 Crear los 5 proyectos Paciente (4 Class Library + 1 xUnit)**
  - DescripciÃ³n: `dotnet new classlib` para `Paciente.Dominio`, `Paciente.Aplicacion`, `Paciente.Infraestructura`, `Paciente.Mensajes`; `dotnet new xunit` para `Paciente.Pruebas`; namespace `DESI.ConsultorioMedico.Paciente.*`; agregarlos a la soluciÃ³n (carpeta `Paciente/`).
  - AceptaciÃ³n: 5 proyectos compilan; `Paciente.Pruebas` es xUnit.

- [x] **1.4 Crear el proyecto WebApi `ConsultorioMedicoWebApi`**
  - DescripciÃ³n: `dotnet new webapi --use-controllers` (`net10.0`); namespace `DESI.ConsultorioMedico.WebApi`; agregar a la soluciÃ³n (raÃ­z).
  - AceptaciÃ³n: el proyecto compila con `dotnet build`.

- [x] **1.5 Configurar las referencias de proyecto**
  - DescripciÃ³n: `Mensajesâ†’Dominio`; `Infraestructuraâ†’{Dominio, Mensajes, Common.Data, Common.Mensajes}`; `Aplicacionâ†’{Infraestructura, Dominio, Mensajes, Common.Mensajes}`; `Pruebasâ†’{Dominio, Aplicacion, Infraestructura, Mensajes, Common.Mensajes}`; `WebApiâ†’{Aplicacion, Infraestructura, Mensajes, Dominio, Common.Data, Common.Http, Common.Helpers, Common.Mensajes}`; `Common.Dataâ†’Microsoft.Data.SqlClient`. Nota: `IPacienteRepository` vive en Infraestructura; `Aplicacionâ†’Infraestructura` para consumir el contrato.
  - AceptaciÃ³n: `dotnet build` de la soluciÃ³n completa en verde.

- [x] **1.6 Agregar paquetes NuGet**
  - DescripciÃ³n: `Common.Data` â†’ `Microsoft.Data.SqlClient`; `Paciente.Pruebas` â†’ `xunit`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`; `ConsultorioMedicoWebApi` â†’ `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`, `Scalar.AspNetCore`.
  - AceptaciÃ³n: `dotnet restore` exitoso; build en verde.

---

## Fase 2: ImplementaciÃ³n â€” Common.Mensajes / Common.Data / Common.Helpers

- [x] **2.1 `ModelResponse<T>` (Common.Mensajes)**
  - DescripciÃ³n: clase pÃºblica genÃ©rica con `IsSuccess (bool)`, `Message (string)`, `Response (T)`.
  - AceptaciÃ³n: build verde; forma JSON `{ IsSuccess, Message, Response }`.

- [x] **2.2 `OperationResult` y `OperationResult<T>` (Common.Mensajes)**
  - DescripciÃ³n: resultado interno con `IsSuccess`, `Message` y `Data` (genÃ©rico solo en `OperationResult<T>`).
  - AceptaciÃ³n: build verde; usado por Infraestructura/Aplicacion (no expuesto al cliente).

- [x] **2.3 `SystemMessageTypes` + `SystemMessage` (Common.Mensajes)**
  - DescripciÃ³n: enum de tipos + catÃ¡logo estÃ¡tico en espaÃ±ol: "Pacientes obtenidos correctamente", "Paciente no encontrado", "Paciente creado exitosamente", "Paciente actualizado exitosamente".
  - AceptaciÃ³n: los 4 mensajes accesibles de forma tipada (nunca hardcodeados en el Controller).

- [x] **2.4 `ConnectionHelper` (Common.Data)**
  - DescripciÃ³n: construye `SqlConnection` desde `ConnectionStrings:DefaultConnection` (via `IConfiguration`).
  - AceptaciÃ³n: build verde; cadena de conexiÃ³n leÃ­da de configuraciÃ³n.

- [x] **2.5 `SqlDataReaderExtensions` (Common.Data)**
  - DescripciÃ³n: extensiÃ³n con `GetStringSafe`, `GetInt32Safe`, `GetDateTimeSafe`, `GetBoolSafe`; manejan `DBNull` (retornan default).
  - AceptaciÃ³n: build verde; lectores tolerantes a columnas nulas.

- [x] **2.6 `BaseRepository` abstracta (Common.Data)**
  - DescripciÃ³n: `IConfiguration` inyectado; `GetConnection()`; `ExecuteReaderAsync<T>(nombreSP, CommandType.StoredProcedure, Func<SqlDataReader,T> mapper, params SqlParameter[])`; `ExecuteNonQueryAsync(nombreSP, params SqlParameter[])` que lee el Id retornado (RETURN/OUTPUT). ConexiÃ³n por operaciÃ³n (`using` + `OpenAsync`), `CommandTimeout` configurable.
  - AceptaciÃ³n: build verde; lista para ser heredada por `PacienteInfraestructura`.

- [x] **2.7 `Common.Helpers` (helpers on-demand)**
  - DescripciÃ³n: crear **solo** los helpers consumidos por Fase 1 (p. ej. `StringHelper` para componer/recortar `NombreCompleto`; `ValidationHelper` solo si las entidades/VOs lo reutilizan). **No** crear `EncryptionHelper` ni helpers de email.
  - AceptaciÃ³n: build verde; sin `EncryptionHelper` ni helpers de email en el proyecto.

---

## Fase 3: ImplementaciÃ³n â€” Paciente.Dominio (puro, sin referencias)

- [x] **3.1 Clase base `Entity<T>`**
  - DescripciÃ³n: abstracta; propiedad `Id` de solo lectura (getter protegido).
  - AceptaciÃ³n: build verde; `Paciente` hereda de `Entity<int>`.

- [x] **3.2 ValueObjects `Email`, `Telefono`, `Sexo`**
  - DescripciÃ³n: `Email` (string?, formato vÃ¡lido, mÃ¡x. 50); `Telefono` (string?, mÃ¡x. 100); `Sexo` (string?, mÃ¡x. 15, solo "Masculino"/"Femenino", **no enum**). Null/empty permitido; valores no nulos se validan y lanzan `ArgumentException` si incumplen.
  - AceptaciÃ³n: build verde; `"ana@"` (Email) y `"Otro"` (Sexo) lanzan `ArgumentException`.

- [x] **3.3 Entidad `Paciente` (Factory Method + Fluent Interface)**
  - DescripciÃ³n: constructor privado + `Create()`; propiedades de solo lectura para los 16 campos (`Id, Nombre, ApellidoPaterno, ApellidoMaterno, Email, FechaAlta, TelefonoCel, Comentarios, Estatus, Edad, Sexo, FechaNacimiento, CreatedBY, CreatedDT, UpdatedBY, UpdatedDT`); mÃ©todos `Setâ€¦` que retornan `this`; cada `Set` valida y lanza `ArgumentException` (p. ej. `SetNombre("")`).
  - AceptaciÃ³n: build verde; `Paciente.Create().SetId(1).SetNombre("Ana")` retorna `this`; `SetNombre("")` lanza `ArgumentException`.

---

## Fase 4: ImplementaciÃ³n â€” Paciente.Mensajes (â†’ Dominio)

- [x] **4.1 `GuardarPacienteRequest`**
  - DescripciÃ³n: 10 campos (`Id, Nombre, ApellidoPaterno, ApellidoMaterno, Email, TelefonoCel, Comentarios, Sexo, FechaNacimiento, Estatus`) con DataAnnotations: `Nombre`/`ApellidoPaterno` `[Required][MaxLength(50)]`; `ApellidoMaterno`/`Email` `[MaxLength(50)]` (Email ademÃ¡s `[EmailAddress]`); `TelefonoCel` `[MaxLength(100)]`; `Comentarios` `[MaxLength(1000)]`; `Sexo` `[MaxLength(15)]`; `Estatus` `[Required]`. Sin `Edad`/`FechaAlta`/auditorÃ­a.
  - AceptaciÃ³n: `Nombre` vacÃ­o â‡’ `ModelState.IsValid == false` ("Nombre es requerido").

- [x] **4.2 `PacienteResponse` + `GetPacientesResponse`**
  - DescripciÃ³n: `PacienteResponse` con 12 campos (`Id, Nombre, ApellidoPaterno, ApellidoMaterno, NombreCompleto, Sexo, Edad, TelefonoCel, Email, Estatus, FechaNacimiento, Comentarios`); `GetPacientesResponse` con `Pacientes (List<PacienteResponse>)` (sin `OperationResult`).
  - AceptaciÃ³n: build verde; `GetPacientesResponse` expone solo la lista.

- [x] **4.3 `PacienteData` (DTO crudo)**
  - DescripciÃ³n: POCO plano con las 16 columnas del SP (`dbo.Paciente`); mapeado desde `SqlDataReader`.
  - AceptaciÃ³n: build verde; usado por Infraestructura y `Mapp`.

- [x] **4.4 `Mapp` (clase estÃ¡tica)**
  - DescripciÃ³n: `MapToPaciente(PacienteData)` (usa Fluent: `Paciente.Create().SetId(â€¦).SetNombre(â€¦).SetEmail(new Email(â€¦))â€¦`), `MapToPacientes(IEnumerable<PacienteData>)`, `MapToPacienteResponse(Paciente)` (con `NombreCompleto = $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim()`; `Edad` int?â†’int coalesce 0; extrae string de los VOs), `MapToPacientesResponse(List<Paciente>)`.
  - AceptaciÃ³n: `NombreCompleto` concatena los tres campos; build verde.

---

## Fase 5: ImplementaciÃ³n â€” Paciente.Aplicacion (â†’ Infraestructura, Dominio, Mensajes, Common.Mensajes)

- [x] **5.1 `IPacienteAplicacion`**
  - DescripciÃ³n: `Task<ModelResponse<GetPacientesResponse>> ObtenerTodos()`, `Task<ModelResponse<PacienteResponse>> ObtenerPorId(int id)`, `Task<ModelResponse<int>> Guardar(GuardarPacienteRequest request)`.
  - AceptaciÃ³n: build verde.

- [x] **5.2 `PacienteAplicacion`**
  - DescripciÃ³n: implementa `IPacienteAplicacion`; inyecta `IPacienteRepository`; orquesta repoâ†’`Mapp`â†’`ModelResponse<T>` con mensajes de `SystemMessage`; `ObtenerPorId` retorna `IsSuccess=false` ("Paciente no encontrado") si es null; `Guardar` traduce `OperationResult<int>`â†’`ModelResponse<int>` (crear vs actualizar segÃºn `Id`).
  - AceptaciÃ³n: build verde; mensajes provienen de `SystemMessage`.

---

## Fase 6: ImplementaciÃ³n â€” Paciente.Infraestructura (â†’ Dominio, Mensajes, Common.Data, Common.Mensajes)

- [x] **6.1 `IPacienteRepository` (contrato, en Infraestructura)**
  - DescripciÃ³n: `Task<List<PacienteData>> GetAllPacientes()`, `Task<PacienteData?> GetPacienteById(int id)`, `Task<OperationResult<int>> SaveOrUpdatePaciente(GuardarPacienteRequest request)`.
  - AceptaciÃ³n: build verde; define el contrato que implementa `PacienteInfraestructura`.

- [x] **6.2 `PacienteInfraestructura` + `GetAllPacientes`**
  - DescripciÃ³n: hereda `BaseRepository`, implementa `IPacienteRepository`; `GetAllPacientes()` ejecuta `GetAllPacientes` (sin parÃ¡metros) con `ExecuteReaderAsync` y mapea filas a `List<PacienteData>` (orden lo define el SP).
  - AceptaciÃ³n: build verde; retorna datos crudos sin lÃ³gica de negocio.

- [x] **6.3 `GetPacienteById`**
  - DescripciÃ³n: ejecuta `GetPacienteById` con `@Id` (int); retorna `PacienteData?` (null si no hay fila).
  - AceptaciÃ³n: build verde; maneja ausencia de registro como `null`.

- [x] **6.4 `SaveOrUpdatePaciente`**
  - DescripciÃ³n: mapea el Request a 10 `SqlParameter` tipados (`@Id, @Nombre, @ApellidoPaterno, @ApellidoMaterno, @Email, @TelefonoCel, @Comentarios, @Sexo, @FechaNacimiento, @Estatus`; nulos como `DBNull.Value`); ejecuta `SaveOrUpdatePaciente`; lee el Id retornado (RETURN/OUTPUT) y retorna `OperationResult<int>`.
  - AceptaciÃ³n: la clase implementa los 3 mÃ©todos del contrato; build verde.

---

## Fase 7: ImplementaciÃ³n â€” WebApi

- [x] **7.1 `PacienteController`**
  - DescripciÃ³n: `GET /api/v1/paciente` (200 `ModelResponse<GetPacientesResponse>`); `GET /api/v1/paciente/{id}` (400 si `id<=0`; 200 encontrado; 404 `IsSuccess=false` "Paciente no encontrado" `Response=null`); `POST /api/v1/paciente/guardar` (400 si `ModelState` invÃ¡lido, sin invocar SP; 200 `ModelResponse<int>`). Incluir comentarios XML (`<summary>`, `<param>`, `<response>`) para Scalar.
  - AceptaciÃ³n: 3 endpoints retornan `ModelResponse<T>`; errores manejados en el Controller.

- [x] **7.2 `Program.cs`**
  - DescripciÃ³n: `AddScoped<IPacienteAplicacion, PacienteAplicacion>()` y `AddScoped<IPacienteRepository, PacienteInfraestructura>()`; Serilog (consola dev / archivo prod); CORS abierto; rate limiter "fixed" (100/min); `AddControllers()` + XML comments; pipeline `UseSerilogRequestLogging()` â†’ `UseCors` â†’ `UseRateLimiter()` â†’ `MapControllers()` â†’ `MapScalarApiReference()`. **Sin JWT.**
  - AceptaciÃ³n: compila; Scalar disponible en `/scalar/v1`.

- [x] **7.3 `appsettings.json` (+ Development)**
  - DescripciÃ³n: `ConnectionStrings:DefaultConnection` (placeholder, sin secretos reales) y configuraciÃ³n de Serilog (consola/archivo).
  - AceptaciÃ³n: `DefaultConnection` legible por `ConnectionHelper`; sin credenciales commiteadas.

---

## Fase 8: Pruebas â€” xUnit (Paciente.Pruebas)

- [x] **8.1 Pruebas de la entidad `Paciente` (`Domain/`)**
  - DescripciÃ³n: Factory/Fluent retornan `this`; `SetNombre("")` y sets con datos invÃ¡lidos lanzan `ArgumentException`; propiedades de solo lectura.
  - AceptaciÃ³n: `dotnet test` en verde para estos casos.

- [x] **8.2 Pruebas de ValueObjects (`Domain/`)**
  - DescripciÃ³n: Email invÃ¡lido (`"ana@"`), Telefono >100, Sexo `"Otro"` lanzan `ArgumentException`; valores nulos permitidos.
  - AceptaciÃ³n: `dotnet test` en verde.

- [x] **8.3 Pruebas de `PacienteAplicacion` (`Application/`)**
  - DescripciÃ³n: con `IPacienteRepository` fake (stub/mock) verificar `ObtenerTodos` (Ã©xito), `ObtenerPorId` (encontrado y no encontrado â‡’ `IsSuccess=false`), `Guardar` (crear `Id=0` vs actualizar `Id>0`).
  - AceptaciÃ³n: `ModelResponse` con forma e `IsSuccess`/`Message` correctos.

- [x] **8.4 Pruebas de `Mapp`**
  - DescripciÃ³n: `MapToPaciente` (crudoâ†’entidad) y `MapToPacienteResponse` incluyendo `NombreCompleto` concatenado.
  - AceptaciÃ³n: `NombreCompleto == "Nombre ApellidoPaterno ApellidoMaterno"` (trim).

---

## Fase 9: VerificaciÃ³n

- [x] **9.1 Build de la soluciÃ³n**
  - DescripciÃ³n: `dotnet build DESI.ConsultorioMedico.sln` en la raÃ­z.
  - AceptaciÃ³n: 0 errores / 0 warnings bloqueantes.

- [x] **9.2 Ejecutar pruebas**
  - DescripciÃ³n: `dotnet test` (proyecto `Paciente.Pruebas`).
  - AceptaciÃ³n: todas en verde.

- [ ] **9.3 Smoke test manual (opcional, requiere BD)**
  - DescripciÃ³n: levantar la API, abrir `/scalar/v1` y probar los 3 endpoints contra los SPs reales (`GetAllPacientes`, `GetPacienteById`, `SaveOrUpdatePaciente`).
  - AceptaciÃ³n: respuestas `ModelResponse<T>` con cÃ³digos 200/400/404 correctos.
