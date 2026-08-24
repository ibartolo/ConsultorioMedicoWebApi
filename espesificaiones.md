# ESPECIFICACIÓN TÉCNICA - DESI.ConsultorioMedico
**Sistema de Consultorio Médico - Modernización 2026**

---

## 1. OBJETIVO DEL PROYECTO
Modernizar la aplicación de consultorio médico migrando de WinForms + ADO.NET (con código fuente perdido) a una arquitectura moderna y mantenible.

**Alcance de esta fase:** 
> **IMPORTANTE:** En esta primera fase solo se desarrollará el **Dominio Paciente**. Los dominios (Cita, Consulta, Usuario, Tratamiento, Anexo) están definidos estructuralmente para futuras fases, pero **no serán generados** en este entregable.

---

## 2. TECNOLOGÍAS Y VERSIONES
| Componente | Tecnología | Versión | Observaciones |
|------------|------------|---------|---------------|
| **IDE** | Visual Studio | 2026 | Incluye soporte nativo para .NET 10 |
| **Backend** | .NET (Core) | 10 | LTS (Soporte extendido) |
| **Frontend** | ASP.NET Core MVC | 10 | Se desarrolla en paralelo pero no en esta especificación |
| **Base de Datos** | SQL Server | 2019+ | Hosting actual: smarteasp (SQL5108.site4now.net) |
| **Acceso a Datos** | ADO.NET | - | Uso estricto de `SqlConnection`, `SqlCommand`, `SqlDataReader`. **No se usará ORM (Entity Framework, Dapper, etc.)** |
| **Logging** | Serilog | Última | Salida a Consola (Desarrollo) y Archivos (Producción) |
| **Documentación API** | Scalar | Última | Reemplaza a Swagger por ser .NET Core. |
| **Pruebas** | xUnit | Última | Proyectos separados por dominio. |
| **Control de Versiones** | Git | - | Repositorio en GitHub/Bitbucket. Rama `main` y `develop`. |

---

## 3. ESTRUCTURA DE LA SOLUCIÓN

**Nombre de la Solución:**
DESI.ConsultorioMedico.sln

**Carpetas y Proyectos a Nivel de Solución:**

DESI.ConsultorioMedico.sln
│
├── Carpeta: Paciente/ (DOMINIO ACTIVO)
│   ├── Proyecto: Paciente.Dominio (Class Library)
│   ├── Proyecto: Paciente.Aplicacion (Class Library)
│   ├── Proyecto: Paciente.Infraestructura (Class Library)
│   ├── Proyecto: Paciente.Mensajes (Class Library)
│   └── Proyecto: Paciente.Pruebas (xUnit)
│
├── Carpeta: Usuario/ (DOMINIO FUTURO)
│   ├── Proyecto: Usuario.Dominio
│   ├── Proyecto: Usuario.Aplicacion
│   ├── Proyecto: Usuario.Infraestructura
│   ├── Proyecto: Usuario.Mensajes
│   └── Proyecto: Usuario.Pruebas
│
├── Carpeta: Cita/ (DOMINIO FUTURO)
│   └── (Misma estructura)
│
├── Carpeta: Consulta/ (DOMINIO FUTURO)
│   └── (Misma estructura)
│
├── Carpeta: Tratamiento/ (DOMINIO FUTURO)
│   └── (Misma estructura)
│
├── Carpeta: Anexo/ (DOMINIO FUTURO)
│   └── (Misma estructura)
│
├── Carpeta: Common/ (COMPARTIDO)
│   ├── Proyecto: Common.Data (Class Library)
│   ├── Proyecto: Common.Http (Class Library)
│   ├── Proyecto: Common.Helpers (Class Library)
│   └── Proyecto: Common.Mensajes (Class Library)
│
└── Proyecto: ConsultorioMedicoWebApi (ASP.NET Core Web API .NET 10)
    ├── Carpeta: Controllers/
    │   └── Archivo: PacienteController.cs
    ├── Carpeta: Middleware/
    ├── Archivo: appsettings.json
    └── Archivo: Program.cs

---

## 4. ESTRUCTURA INTERNA DE CADA DOMINIO (Aplicable a Paciente y futuros)

### 4.1. Proyecto: [Dominio].Dominio
**Tipo:** Class Library (.NET 10)
**Propósito:** Entidades, ValueObjects, reglas de negocio.

**Carpetas Internas:**
- Entities/ → Contiene las entidades del dominio (ej. Paciente.cs)
- ValueObjects/ → Contiene objetos de valor (ej. Email.cs, Telefono.cs)
- Interfaces/ → Contiene contratos de repositorios (ej. IPacienteRepository.cs)

**Características de las Entidades:**
- Heredan de Entity<T> (con Id).
- Constructor privado.
- Propiedades con getters públicos (solo lectura).
- Métodos Set que devuelven this (Fluent Interface).
- Validaciones en cada método (lanzan ArgumentException).

### 4.2. Proyecto: [Dominio].Aplicacion
**Tipo:** Class Library (.NET 10)
**Propósito:** Casos de uso, servicios, orquestación.

**Archivos Mínimos:**
- I[Dominio]Aplicacion.cs (Interfaz)
- [Dominio]Aplicacion.cs (Implementación)

**Responsabilidades:**
- Recibir datos del Controller.
- Llamar a Infraestructura para ejecutar SPs.
- Recibir datos crudos de Infraestructura.
- Llamar a Mapp (en Mensajes) para convertir a entidades.
- Armar objetos Response con datos + OperationResult.
- Devolver al Controller.

### 4.3. Proyecto: [Dominio].Infraestructura
**Tipo:** Class Library (.NET 10)
**Propósito:** Acceso a datos, ejecución de SPs.

**Archivos Mínimos:**
- I[Dominio]Infraestructura.cs (Interfaz)
- [Dominio]Infraestructura.cs (Implementación)

**Responsabilidades:**
- Heredar de BaseRepository (en Common.Data).
- Ejecutar SPs con SqlConnection, SqlCommand, SqlDataReader.
- Devolver datos "crudos" (DataReader mapeado a objetos simples).
- NO hacer lógica de negocio.

### 4.4. Proyecto: [Dominio].Mensajes
**Tipo:** Class Library (.NET 10)
**Propósito:** Objetos de transferencia y mapeo.

**Carpetas Internas:**
- Requests/ → DTOs de entrada (ej. CrearPacienteRequest.cs)
- Responses/ → DTOs de salida (ej. GetPacientesResponse.cs)
- Archivo: Mapp.cs (Clase estática)

**Clase Mapp:**
- Clase public static.
- Métodos: MapXToY() que reciben objetos crudos y retornan entidades de dominio.
- Usa Fluent Interface de las entidades (.Create().SetX().SetY()).
- Retorna List<Entidad> o Entidad según corresponda.

**Requests:** Son DTOs de entrada (desde el cliente).
**Responses:** Son DTOs de salida que incluyen:
- Datos (lista o entidad).
- OperationResult (de Common.Mensajes).

### 4.5. Proyecto: [Dominio].Pruebas
**Tipo:** xUnit
**Propósito:** Pruebas unitarias por flujo de aplicación.

**Carpetas Internas:**
- Domain/ → Pruebas de entidades y value objects.
- Application/ → Pruebas de servicios.
- Infrastructure/ → Pruebas de repositorios.

---

## 5. PROYECTO COMMON

### 5.1. Proyecto: Common.Data
**Tipo:** Class Library (.NET 10)
**Propósito:** Conexión a BD y repositorio base.

**Archivos Mínimos:**
- BaseRepository.cs (Clase abstracta)
- Extensions/SqlDataReaderExtensions.cs
- Helpers/ConnectionHelper.cs

**BaseRepository:** 
- Método GetConnection() → retorna SqlConnection.
- Métodos ExecuteReaderAsync<T>().
- Métodos ExecuteNonQueryAsync().
- Configuración desde appsettings.json.

### 5.2. Proyecto: Common.Http
**Tipo:** Class Library (.NET 10)
**Propósito:** Cliente HTTP genérico para futuras integraciones.

**Archivos Mínimos:**
- HttpClientBase.cs
- Extensions/HttpResponseExtensions.cs

### 5.3. Proyecto: Common.Helpers
**Tipo:** Class Library (.NET 10)
**Propósito:** Utilidades varias.

**Archivos Mínimos:**
- DateTimeHelper.cs
- StringHelper.cs
- ValidationHelper.cs
- EncryptionHelper.cs (para encriptar/desencriptar contraseñas)

### 5.4. Proyecto: Common.Mensajes
**Tipo:** Class Library (.NET 10)
**Propósito:** Objetos de mensajería compartidos.

**Archivos Mínimos:**
- OperationResult.cs
- OperationResultT.cs
- SystemMessage.cs
- SystemMessageTypes.cs (enum)

---

## 6. PROYECTO WEB API

### 6.1. Proyecto: ConsultorioMedicoWebApi
**Tipo:** ASP.NET Core Web API (.NET 10)
**Propósito:** Exponer endpoints REST.

**Carpetas Internas:**
- Controllers/ → PacienteController.cs (y futuros)
- Middleware/ → ExceptionHandlingMiddleware.cs, LoggingMiddleware.cs
- Filters/ → Filtros personalizados
- Archivo: appsettings.json
- Archivo: appsettings.Development.json
- Archivo: Program.cs

### 6.2. Configuración de Program.cs
- Inyección de dependencias (Scoped).
- CORS (permitir cualquier origen por ahora).
- Scalar para documentación interactiva.
- Configuración de Serilog.
- Rate Limiting (100 requests por minuto).
- Autenticación JWT Bearer.

### 6.3. Documentación Interactiva (Scalar)
Al tratarse de una API desarrollada con .NET 10 (Core) y no con .NET Framework, se utilizará Scalar como herramienta de documentación interactiva en lugar de Swagger.

**Configuración:**
- Paquete NuGet: Scalar.AspNetCore
- Configuración en Program.cs con app.MapScalarApiReference()
- Endpoint de documentación: /scalar/v1

### 6.4. Documentación de Endpoints para Scalar
Cada método de acción en los controladores deberá incluir comentarios XML con las etiquetas <summary>, <param> y <response> para que Scalar pueda generar la documentación interactiva completa. Esto aplica para todos los endpoints del dominio Paciente.

### 6.5. Endpoint de Autenticación
El endpoint /api/v1/Usuario/Autenticacion será responsabilidad del dominio Usuario.
- Recibirá LoginRequest (UserName, Password).
- Validará contra la BD mediante el SP Autentication.
- En caso de éxito, generará y retornará un JWT Bearer Token.

---

## 7. FLUJOS DE DATOS

### 7.1. Flujo de Obtener Pacientes (Ejemplo)
1. Cliente → GET /api/v1/paciente
2. Controller → PacienteAplicacion.ObtenerTodos()
3. PacienteAplicacion → PacienteInfraestructura.GetAllPacientes()
4. PacienteInfraestructura → SP: GetAllPacientes (SqlDataReader)
5. PacienteInfraestructura → retorna List<PacienteObj> (datos crudos)
6. PacienteAplicacion → Mapp.MapToPacienteInfo(listaCruda)
7. Mapp → retorna List<Paciente> (entidades de dominio)
8. PacienteAplicacion → arma ModelResponse<List<Paciente>> con:
   - IsSuccess = true
   - Message = "Pacientes obtenidos correctamente"
   - Response = List<Paciente>
9. Controller → retorna ModelResponse<List<Paciente>>
10. Cliente → recibe JSON

### 7.2. Flujo de Guardar Paciente (Ejemplo)
1. Cliente → POST /api/v1/paciente (body: CrearPacienteRequest)
2. Controller → PacienteAplicacion.Crear(request)
3. PacienteAplicacion → PacienteInfraestructura.SaveOrUpdatePaciente()
4. PacienteInfraestructura → SP: SaveOrUpdatePaciente
5. PacienteInfraestructura → retorna Id generado
6. PacienteAplicacion → arma ModelResponse<int> con:
   - IsSuccess = true
   - Message = "Paciente creado exitosamente"
   - Response = Id
7. Controller → retorna ModelResponse<int>

### 7.3. Flujo de Error Controlado
1. Cliente → GET /api/v1/paciente/9999
2. Controller → PacienteAplicacion.ObtenerPorId(9999)
3. PacienteAplicacion → PacienteInfraestructura.GetPacienteById(9999)
4. PacienteInfraestructura → retorna null (no encontrado)
5. PacienteAplicacion → arma ModelResponse<Paciente> con:
   - IsSuccess = false
   - Message = "Paciente no encontrado"
   - Response = null
6. Controller → retorna 404 Not Found con ModelResponse

### 7.4. Flujo de Autenticación (Login)
1. Cliente → POST /api/v1/Usuario/Autenticacion (body: LoginRequest)
2. Controller (Usuario) → UsuarioAplicacion.Autenticar(request)
3. UsuarioAplicacion → UsuarioInfraestructura.ValidarCredenciales(UserName, Password)
4. UsuarioInfraestructura → SP: Autentication
5. SP → Desencripta Password de BD y compara con la ingresada
6. UsuarioInfraestructura → retorna datos del usuario
7. UsuarioAplicacion → valida Estatus = 1
8. Si es exitoso: genera JWT con NombreUsuario, IdUsuario, IdEmpresa
9. Retorna ModelResponse<string> con el token
10. Si falla: retorna ModelResponse con IsSuccess = false y Message = "No fue posible autenticarse"

---

## 8. PROCEDIMIENTOS ALMACENADOS POR DOMINIO

### 8.1. Dominio Paciente (ACTIVO)
- GetAllPacientes
- GetPacienteById
- GetPacientesFiltrados
- GetAllPacienteByEstatus
- GetPacienteByFechaNacimiento
- SaveOrUpdatePaciente

### 8.2. Dominio Usuario (FUTURO)
- Autentication (existente, se ajustará para desencriptar contraseña)

### 8.3. Dominio Cita (FUTURO)
- GetAllCitaByPacienteID
- GetAllCitaByTratamientoId
- GetAllCitaByTratamientoIdForPublicidad
- GetAllCitaTratamiento
- GetCitaByFechaCita
- GetCitaByPaciente
- GetCitabyPacienteIDandFechaActual
- GetCitaTratamientoById
- SaveOrUpdateCita

### 8.4. Dominio Consulta (FUTURO)
- GetAllConsultaByPacienteID
- GetAllConsultaByTipoConsultaId
- GetAllConsultaCalendario
- GetConsultaByFechaConsulta
- GetConsultaByPaciente
- GetConsultaCalendarioByIdConsulta
- GetConsultaMayor
- SaveOrUpdateConsulta
- GetRecetaByConsulta

### 8.5. Dominio Anexo (FUTURO)
- GetAllAnexoByPacienteID

---

## 9. CONVENCIONES DE CÓDIGO

### 9.1. Nombres de Proyectos
- [Dominio].Dominio
- [Dominio].Aplicacion
- [Dominio].Infraestructura
- [Dominio].Mensajes
- [Dominio].Pruebas

### 9.2. Nombres de Clases e Interfaces
- Aplicacion: [Dominio]Aplicacion (clase), I[Dominio]Aplicacion (interfaz)
- Infraestructura: [Dominio]Infraestructura (clase), I[Dominio]Infraestructura (interfaz)

### 9.3. Namespaces
- DESI.ConsultorioMedico.[Dominio].Dominio
- DESI.ConsultorioMedico.[Dominio].Aplicacion
- DESI.ConsultorioMedico.[Dominio].Infraestructura
- DESI.ConsultorioMedico.[Dominio].Mensajes
- DESI.ConsultorioMedico.Common.[Capa]
- DESI.ConsultorioMedico.WebApi.Controllers

### 9.4. Patrones de Diseño
- Fluent Interface: Métodos Set en entidades que retornan this.
- Factory Method: Método Create en entidades (constructor privado).
- Repository: Capa de Infraestructura.
- Dependency Injection: Inyección de dependencias en Program.cs.
- DTO: Requests y Responses en Mensajes.
- Static Mapper: Clase Mapp en Mensajes.

### 9.5. Manejo de Errores
El manejo de errores se realizará exclusivamente a nivel de Controller (no se implementará Middleware global).
- Cada método de acción validará el ModelState para errores de validación (400).
- Los errores de negocio (como "paciente no encontrado") se manejarán dentro de la capa de Aplicación y se retornarán mediante ModelResponse<T> con IsSuccess = false.
- Las excepciones no controladas serán capturadas por el framework y retornarán un error 500 genérico.

---

## 10. CONFIGURACIÓN Y DESPLIEGUE

### 10.1. appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SQL5108.site4now.net;Database=db_9c7990_consultadesi;User Id=db_9c7990_consultadesi_admin;Password=****;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "DESI_Consultorio_2026_SecretKey_Segura",
    "Issuer": "DESI.ConsultorioMedico",
    "Audience": "DESI.ConsultorioMedico.Users",
    "ExpirationHours": 8
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

### 10.2. Configuración de JWT
- SecretKey: "DESI_Consultorio_2026_SecretKey_Segura"
- Issuer: "DESI.ConsultorioMedico"
- Audience: "DESI.ConsultorioMedico.Users"
- ExpirationHours: 8 (configurable como entero)

### 10.3. Logging con Serilog
Se implementará Serilog para el registro de eventos y errores.
- Desarrollo: Configuración para consola (WriteTo.Console()).
- Producción: Configuración para archivos (WriteTo.File()) con rotación diaria.
- Se registrarán: requests entrantes, errores controlados, excepciones no controladas y eventos de autenticación.

### 10.4. CORS
Por ahora se configurará con política abierta:
```csharp
app.UseCors(policy => policy.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
							
### 10.5. Rate Limiting
Se implementará Rate Limiting con 100 requests por minuto:

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

### 11. DEFINICIONES FINALES PARA LA API

Tema	Decisión
Versionado	v1 en todos los endpoints (ej. /api/v1/paciente)
Contraseña	Se almacena encriptada en BD. Al autenticar, se desencripta y se compara con la ingresada.
SP Autenticación	Se mantiene Autentication pero se ajusta la lógica: recibe UserName y Password en texto plano, consulta el registro por UserName, desencripta el campo Password de la BD y compara con el ingresado.
JWT	Contiene: NombreUsuario, IdUsuario, IdEmpresa
Expiración	8 horas (configurable en appsettings.json como entero)
Endpoint Login	/api/v1/Usuario/Autenticacion (Request: UserName, Password / Response: toda la info del usuario + token)
Nombres endpoints	En español, basados en métodos de aplicación y SPs
Paginación	No se manejará en esta fase
Borrado	Siempre lógico (campo Estatus)
Response	ModelResponse<T> (IsSuccess, Message, Response)
Manejo errores login	"No fue posible autenticarse"
Pruebas	Por flujo de aplicación, dentro de cada dominio
ModelResponse<T>	IsSuccess (bool), Message (string), Response (T)
OperationResult	Se usa internamente en capas de aplicación/infraestructura
ModelResponse	Es el response público que ve el cliente