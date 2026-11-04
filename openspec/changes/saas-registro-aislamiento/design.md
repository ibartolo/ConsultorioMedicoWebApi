# Design: Registro SaaS de Consultorio + Aislamiento Multi-Tenant

## Technical Approach

Primera entrega SaaS del Consultorio Médico. Dos frentes: (1) **registro público** que crea un tenant (`DatosInstitucion`) + fila fiscal 1:1 (`DatosFiscales`) + primer admin (`Usuario`, `UserName`=RFC) en una sola transacción, y (2) **aislamiento multi-tenant** por `ConsultorioID` derivado del usuario autenticado (claim `empresaId` en WebApi, `TokenCookie.EmpresaID` en MVC), nunca del cliente. Se mantiene el flujo View → MVC → HttpClientConnection → WebApi → DbWrapper → SP; se agregan `@ConsultorioID` a los SPs y se validan pertenencia en todos los módulos. Se portan PBKDF2 (`Cryptography`) y el rehash legado transparente. Todo es aditivo e idempotente; solo `develop`/`main` con el clon `db_9c7990_consultoriodesitst`; `-dermatologia` y `DB_9C7990_Consultorio` intactas.

> **Decisión prevaleciente sobre specs**: el delta `paciente/spec.md` menciona Factory Method/Fluent Interface para la entidad `Paciente`, pero la decisión aprobada lo **EXCLUYE** ("solo agregar `ConsultorioID` y filtro; no cambiar el patrón"). Se sigue la decisión aprobada: `Paciente` conserva su POCO + `BaseObject` actual y solo gana `ConsultorioID`.

---

## 1. Modelo de datos y orden exacto de migración

El tenant es `DatosInstitucion` (`Id` = id del consultorio). `Usuario.CompaniaID` es FK al tenant. `DatosFiscales.DatosInstitucionID` es 1:1 con `DatosInstitucion`. Los catálogos/dominio llevan `ConsultorioID` (FK → `DatosInstitucion.Id`).

### Scripts nuevos (solo en el clon develop/main)
- `sql/migraciones_saas_01_esquema.sql` — columnas, FKs, backfills, unique indexes, corrección `CitaPaquete`.
- `sql/migraciones_saas_02_procedimientos.sql` — `ALTER PROCEDURE` + `CREATE` nuevos.
- `sql/migraciones_saas_03_archivos.sql` — actualización de rutas `LogoRuta`/`Anexo.Ruta`.

### Orden (idempotente, sin DROP de tablas/SPs; solo `DROP CONSTRAINT` para corregir FKs)
1. `IF COL_LENGTH('dbo.<Tabla>','ConsultorioID') IS NULL` → `ALTER TABLE ADD ConsultorioID INT NULL` en las **11 tablas**: `Paciente`, `Cita`, `Consulta`, `Anexo`, `PacientePaquete`, `PagoPaquete`, `CitaPaquete`, `Tratamiento`, `TratamientoPaquete`, `Paquete`, `TipoConsulta`.
2. Backfill: `UPDATE <Tabla> SET ConsultorioID = 1 WHERE ConsultorioID IS NULL` (11 tablas; `DatosInstitucion.Id=1` ya existe).
3. FK por tabla: `IF NOT EXISTS (sys.foreign_keys name='FK_<Tabla>_ConsultorioID')` → `ALTER TABLE ADD CONSTRAINT ... FOREIGN KEY (ConsultorioID) REFERENCES DatosInstitucion(Id)`.
4. `IF EXISTS (sys.columns ... is_nullable=1)` → `ALTER TABLE ALTER COLUMN ConsultorioID INT NOT NULL` (las 11).
5. `Usuario.Password`: `ALTER COLUMN Password NVARCHAR(250) NULL` (idempotente vía `sys.columns.max_length`).
6. `Usuario.CompaniaID`: backfill `NULL→1` (solo `ivanbartolo`), luego `ALTER COLUMN CompaniaID INT NOT NULL`, luego FK → `DatosInstitucion(Id)`.
7. Corrección `CitaPaquete` (vacía): descubrir nombres reales de constraints vía `sys.foreign_keys` + `sys.foreign_key_columns`; `DROP` las FKs malas y `ADD` las correctas — `FK_CitaPaquete_Cita → Cita(Id)` (hoy apunta a `Paciente`) y `FK_CitaPaquete_PacientePaquete → PacientePaquete(Id)` (hoy apunta a `Paquete`). Guard de idempotencia por `referenced_object_id`.
8. Unique indexes (`IF NOT EXISTS sys.indexes`): `UX_Usuario_UserName`, `UX_DatosFiscales_RFC`, `UX_DatosFiscales_RazonSocial`, `UX_DatosFiscales_DatosInstitucionID` (1:1), `UX_DatosInstitucion_Nombre`, `UX_DatosInstitucion_Email`. Todos `UNIQUE` (los opcionales con `WHERE col IS NOT NULL`).

**Defaults**: `ConsultorioID` sin default (lo asigna siempre el servicio desde el claim). `Pais` default "México" aplicado en el servicio (no en schema).

---

## 2. Estrategia de SPs

Regla: **no DROP**; los existentes se modifican con `ALTER PROCEDURE` (naturalmente idempotente); los nuevos se crean con `IF OBJECT_ID(...) IS NULL ... CREATE` (patrón ya usado en el repo). Los servicios pasan el tenant como parámetro explícito (`@ConsultorioID` o `@CompaniaID`), obtenido del claim — nunca del body/query del cliente.

### SPs a ALTER (agregar filtro/inserción por tenant)
| SP | Cambio |
|----|--------|
| `sp_nh_GetAllCitas`, `sp_nh_GetCitaById`, `sp_nh_SaveCita`, `sp_nh_DeleteCita` | +`@ConsultorioID`; `WHERE ... AND ConsultorioID=@ConsultorioID`; `SaveCita` inserta `ConsultorioID`; `DeleteCita` acotado (baja lógica `Estatus=0`) |
| `GetAllCitaByPacienteID` | +`@ConsultorioID` |
| `sp_nh_GetAllConsultas`, `sp_nh_GetConsultaById`, `sp_nh_SaveConsulta`, `GetAllConsultaByPacienteID` | +`@ConsultorioID` |
| `sp_nh_GetPacienteById`, `sp_nh_SavePaciente`, `sp_nh_DeletePaciente`, `GetAllPacienteByEstatus` | +`@ConsultorioID` |
| `sp_nh_GetAllUsuarios`, `sp_nh_GetUsuarioById` | +`@CompaniaID` (filtro por consultorio) |
| `sp_nh_SaveUsuario` | sin cambio de firma; el servicio asigna `CompaniaID` del claim y hashea |
| `sp_nh_GetDatosFiscales`, `sp_nh_GetDatosFiscalById`, `sp_nh_SaveDatosFiscal`, `sp_nh_DeleteDatosFiscal` | +`@ConsultorioID` (= `DatosInstitucionID`); elimina "Unico" global |
| `GetDatosInstitucion`, `SaveOrUpdateDatosInstitucion` | resolver por `@Id` del tenant (el servicio fija `Id` desde el claim) |
| `sp_nh_GetAnexoById`, `sp_nh_SaveAnexo` | +`@ConsultorioID` (y la consulta inline de listado en `DbWrapper.Anexo.cs`) |
| `sp_nh_GetPaqueteById`, `sp_nh_SavePaquete`, `GetAllPaqueteDT` | +`@ConsultorioID` |
| `sp_nh_GetTratamientoById`, `sp_nh_SaveTratamiento`, `GetAllTratamiento` | +`@ConsultorioID` |
| `GetAllTipoConsulta` | +`@ConsultorioID` |
| `sp_nh_GetTratamientosPorPaciente` | +`@ConsultorioID` |
| `sp_nh_GetRecetaByConsulta` | ya recibe `@institucionId`; el servicio lo fija desde el claim (y valida `Consulta.ConsultorioID`) |
| `sp_nh_AutenticacionUsuario` | **redefinir**: filtro solo `UserName` (sin filtrar `Estatus`); devuelve `Password` y `Estatus` para verificar en C#; se elimina la comparación de password en SQL |

### SPs nuevos (CREATE + IF OBJECT_ID)
- `sp_nh_GetPaquetesPorPaciente (@PacienteId, @ConsultorioID)` y `sp_nh_GetCitasPorPacientePaquete (@PacientePaqueteId, @ConsultorioID)` — lectura filtrada (spec `paquetes-lectura`).
- `sp_nh_GetTipoConsultaById`, `sp_nh_SaveTipoConsulta`, `sp_nh_DeleteTipoConsulta` (baja lógica) — CRUD `TipoConsulta` con `@ConsultorioID`.
- `sp_nh_UpdateUsuarioPassword (@Id, @Password)` — rehash transparente sin tocar otros campos.
- `sp_nh_ValidarRegistroUnico` (opcional) — o validación inline en `DbWrapper.Registro.cs` (ver §3).

### Cómo pasan el tenant los servicios
WebApi: `BaseController.ObtenerEmpresaIdDesdeClaim()` (ya existe, `long`) → se inyecta en cada llamada a `DbWrapper` como `SqlParameter("@ConsultorioID", empresaId)`. Los controllers dejan de recibir el tenant del request; los `id` de ruta se validan contra el tenant vía el `WHERE ... AND ConsultorioID` del SP (si no coincide → `null` → `ModelResponse IsSuccess=false`).

---

## 3. Arquitectura de registro

### Flujo
```
Registro/Index.cshtml (público) ──serialize──> RegistroController (MVC)
  ──> RegistroService (MVC, valida jQuery/validator) ──> HttpClientConnection.Registro
  ──> RegistroController (WebApi, [AllowAnonymous]) ──> RegistroService (WebApi)
  ──> DbWrapper.Registro (BeginTransaction → 3 SPs → Commit/Rollback)
```

### Validación y unicidad (sin fallback)
- **MVC y WebApi** validan con `RegistroValidator` compartido (obligatorios: `NombreComercial`, `RazonSocial`, `RFC`, `Responsable`, `Direccion`→`Calle`, `CorreoContacto`, `Password`+`Confirmacion`; email con regex; `CP` int D5; RFC con `[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}`).
- RFC normalizado: `trim().ToUpper()` → `UserName`.
- Unicidad pre-check en `DbWrapper.Registro` (queries inline parametrizadas, patrón ya usado en `Anexo`/`Dashboard`): RFC (`DatosFiscales.RFC`), email (`DatosInstitucion.Email`), `NombreComercial` (`DatosInstitucion.Nombre`), `RazonSocial` (`DatosFiscales.RazonSocial`), `UserName` (`Usuario.UserName`). Cada duplicado devuelve mensaje específico en español. Los unique indexes son la red final.

### Transacción (patrón HelpDesk)
Un solo `DbWrapper` reutilizando la transacción "ambiente" de `BaseDbWrapper` (`BeginTransaction`/`CommitTransaction`/`RollbackTransaction`; `ExecuteScalar` ya enhebra `_transaction`):
1. `BeginTransaction()`.
2. `SaveOrUpdateDatosInstitucion` con `@Id=0` (inserción; `Pais="México"`) → obtiene `nuevoId`.
3. `sp_nh_SaveDatosFiscal` con `@DatosInstitucionID=nuevoId` (una fila fiscal).
4. `sp_nh_SaveUsuario` con `@CompaniaID=nuevoId`, `@UserName=RFC`, `@Password=PBKDF2`.
5. `CommitTransaction()`; cualquier excepción → `RollbackTransaction()` → `ModelResponse IsSuccess=false`.

**Hashing**: `Password` se hashea en WebApi con `Helpers/Cryptography.cs` **antes** de guardar (no se envía temporal). Confirmación solo se valida en MVC y se descarta antes de enviar a la API.

### Redirect/login
Al éxito, el MVC muestra Swal y redirige a `Home/Autentication` (login neutro). No hay auto-login.

---

## 4. Diseño de autenticación

- **`Cryptography.cs`** (`ConsultorioMedicoWebApi/Helpers/`): portar de HelpDesk — PBKDF2 (`Rfc2898DeriveBytes`, SHA256), formato `PBKDF2$10000$<saltBase64>$<hashBase64>`, salt 16 B, hash 32 B, 10000 iteraciones. Métodos `HashPassword(string)` y `Verify(string password, string hash)` (detecta prefijo `PBKDF2$`).
- **Login**: `AutenticacionController.Autenticar` → `AutenticacionService.AutenticarUsuario(userName, password)`:
  1. `DbWrapper.ObtenerUsuarioPorUserName(userName)` (SP alterado: filtra solo por `UserName` y devuelve `Estatus`; no filtra `Estatus` para poder distinguir inactivo de inexistente).
  2. Si `null` → "Usuario o contraseña incorrectos" (genérico). Si `Estatus=0` → rechazo con mensaje claro.
  3. Verificar en C#: si `Password` empieza con `PBKDF2$` → `Cryptography.Verify`; si no → comparación texto plano (legado).
  4. **Rehash transparente**: si era legado y verifica, `sp_nh_UpdateUsuarioPassword(id, hash)` y devolver DTO.
- **Token OAuth**: `Startup.GrantResourceOwnerCredentials` **debe dejar de usar** `DbWrapper.AutenticarUsuario` (texto plano) y usar la misma autenticación PBKDF2 (`AutenticacionService` o helper compartido), de lo contrario `/token` seguiría aceptando texto plano. Emite claims `usuarioId` y `empresaId` (= `CompaniaID`, ahora NOT NULL). El token **no es JWT** (blob opaco cifrado con `machineKey`); no cambia.
- **MVC**: `Home.LogIn` ya arma `TokenCookie.EmpresaID` desde `usuario.CompaniaID`; sin cambios de flujo salvo que el backend valida PBKDF2. Cookie FormsAuth con `UserData` JSON (ya implementado).
- **Pantalla Usuario**: `UsuarioService.GuardarOActualizarUsuario` (WebApi) hashea el password si es nuevo o no vacío; si edición con password vacío conserva el actual (no llama `UpdatePassword`). `Estatus=1` por defecto en alta. `CompaniaID` se asigna del claim (ignora el valor del cliente).
- **Login sin branding**: `Home/Autentication` no muestra logo/nombre (ya es neutro; verificar que no inyecte nada del tenant).

---

## 5. Resolución y obligación de tenant

- **WebApi** (`BaseController.ObtenerEmpresaIdDesdeClaim()`): ya existe. Todos los controllers pasan `empresaId` a sus servicios; los servicios lo inyectan en `@ConsultorioID`/`@CompaniaID`. Los `id` de ruta que no pertenezcan al tenant → SP no devuelve fila → `IsSuccess=false` con mensaje claro (nunca datos ajenos).
- **MVC** (`TokenCookie.EmpresaID`): `BaseController` ya lo expone. `DatosInstitucion`/`DatosFiscales`/`Consulta` controllers ya lo leen (mantener; eliminar el fallback `: 1` en `DatosInstitucion`/`DatosFiscales` para que no dependan de un id de cliente — el WebApi lo fuerza por claim).
- **Entidades/DTO**: agregar `ConsultorioID int` a `Paciente`, `Cita`, `Consulta`, `Anexo`, `Tratamiento`, `Paquete`, `TipoConsulta` (+ `TipoConsultaDTO`). Las tablas puente (`PacientePaquete`, `PagoPaquete`, `CitaPaquete`, `TratamientoPaquete`) reciben la columna en la migración; sus escrituras quedan acotadas por los SPs de sus módulos.
- **Dashboard/Historial/Receta**: `DbWrapper.Dashboard` (SQL inline) y `DbWrapper.Historial` agregan filtro por `ConsultorioID`; `sp_nh_GetRecetaByConsulta` fija `@institucionId` desde el claim.
- **Usuarios**: listar/filtrar por `CompaniaID`; el alta asigna `CompaniaID` automáticamente; el cliente no elige consultorio.

---

## 6. Cambios de UI

- **Registro público**: `Views/Registro/Index.cshtml` (layout `_LayoutPublic`), Bootstrap 5 + jQuery Validate, `serialize()`, hidden inputs; campos según spec; mensajes de error en español vía Swal.
- **Login**: sin branding (ya neutro). Se agrega `"Registro.Index","Registro.Guardar"` a `PublicActions` en `FilterConfig.AuthenticationFilter`.
- **Sidebar/receta branding**: `_Layout.cshtml` reemplaza "Consultorio Médico" hardcodeado (header + navbar-top) por `DatosInstitucion.Nombre`/`LogoRuta` del tenant (poblado desde `TokenCookie.EmpresaID` al cargar). La receta ya resuelve `DatosInstitucion` por `@institucionId` (recibir el del tenant).
- **DatosInstitucion/DatosFiscales**: vistas existentes operan sobre el tenant actual (controllers ya usan `tokenCookie.EmpresaID`); el logo se sube/descarga bajo `Attachment/{consultorioId}/institucion/`.
- **Usuario**: `Nuevo.cshtml` agrega confirmación de password; `CompaniaID` ya no es editable por el cliente.
- **TipoConsulta CRUD**: nuevas `Views/TipoConsulta/Index.cshtml` + `Nuevo.cshtml` (listar/crear/editar/desactivar soft), siguiendo el patrón de `Tratamiento`.
- **Tratamiento/Paquete**: sin cambios visuales (el filtro es por backend/SP).
- **Anexo modal/API**: `_AnexosModal.cshtml` y endpoints de Anexo validan tenant y usan la nueva ruta; descarga por proxy MVC.

---

## 7. Migración de almacenamiento de archivos

- **Layout nuevo**: `Attachment/{consultorioId}/{pacienteId}/[{consultaId}/]{guid}.ext`; logo en `Attachment/{consultorioId}/institucion/{guid}.ext`.
- **Movimiento físico** (paso manual/apply, no SQL): `Attachment/institucion/` → `Attachment/1/institucion/`; carpetas numéricas existentes (`Attachment/3627/...`, huérfanas) → `Attachment/1/3627/...`.
- **SQL** (`03_archivos.sql`, idempotente): `UPDATE DatosInstitucion SET LogoRuta = REPLACE(...) WHERE LogoRuta LIKE '/Attachment/institucion/%'`; `UPDATE Anexo SET Ruta = REPLACE(Ruta,'/Attachment/','/Attachment/1/') WHERE Ruta LIKE '/Attachment/%' AND Ruta NOT LIKE '/Attachment/1/%'` (hoy 0 filas, seguro).
- **Bloqueo directo**: `Attachment/Web.config` con `<system.web><authorization><deny users="*"/></authorization></system.web>` + restricción en `<system.webServer>`; todo list/upload/download pasa por API con validación de tenant.

---

## 8. Riesgos, rollback, verificación, out-of-scope

### Riesgos
| Riesgo | Prob. | Mitigación |
|--------|-------|-----------|
| Backfill masivo a 1 corrompe datos | Media | Idempotente con `WHERE ConsultorioID IS NULL`; respaldo previo del clon; solo develop/main |
| `/token` sigue en texto plano (olvido en `Startup`) | Alta | Refactor obligatorio a PBKDF2 en `GrantResourceOwnerCredentials`; prueba con `admin`/`ivanbartolo` |
| FKs `CitaPaquete` con nombres desconocidos | Alta | Descubrir nombres reales vía `sys.foreign_keys`; corregir antes de escritura |
| Fuga de tenant en algún endpoint | Alta | Revisión por módulo; tenant solo del claim; prueba de suplantación (`empresaId=3` en body ignorado) |
| Rehash legado falla | Media | Fallback texto plano → rehash; rollback de auth sin bloqueo |
| Idempotencia de ALTER | Media | Guards `sys.columns`/`sys.indexes`/`sys.foreign_keys` |

### Rollback
Todo aditivo y versionable → `git checkout` de archivos nuevos/modificados. Migraciones solo en el clon previa **aprobación explícita del usuario**; para revertir datos, restaurar respaldo del clon. Login conserva fallback a texto plano hasta confirmar rehash.

### Verificación
`MSBuild Debug` (solución completa) y `aspnet_compiler -p ConsultorioMedicoMVC` (ambos **exit 0**). Sin proyecto de tests. Pruebas funcionales manuales: registro completo, RFC duplicado, rollback, login PBKDF2, rehash, aislamiento entre consultorios 1 y 2, CRUD TipoConsulta, branding y descarga de anexos.

### Out-of-scope
Roles/permisos, trial/vigencia/licencia/pagos/límites (solo 2 pilotos), superadmin, seeds de catálogos, refactor Factory Method de `Paciente`, ramas/BD `-dermatologia`, BD productiva SaaS.

---

## 9. Mapa de cambios archivo por archivo

### Entities (`ConsultorioMedicoEntities`)
| Archivo | Acción |
|---------|--------|
| `Catalogos/Paciente.cs`, `Catalogos/PacienteDTO.cs` | Mod | +`ConsultorioID` (mantener patrón actual) |
| `Catalogos/Cita.cs`, `Catalogos/CitaDTO.cs` | Mod | +`ConsultorioID` |
| `Catalogos/Consulta.cs`, `Catalogos/ConsultaDTO.cs` | Mod | +`ConsultorioID` |
| `Catalogos/Anexo.cs` | Mod | +`ConsultorioID` |
| `Catalogos/Tratamiento.cs`, `Catalogos/Paquete.cs` | Mod | +`ConsultorioID` |
| `Catalogos/TipoConsulta.cs` | Mod | +`ConsultorioID` |
| `Catalogos/TipoConsultaDTO.cs`, `Catalogos/TipoConsultaValidator.cs` | New | DTO + validador CRUD |
| `Registro/RegistroDTO.cs`, `Registro/RegistroValidator.cs` | New | DTO de captura + validador compartido (obligatorios, RFC, email, CP) |
| `Catalogos/PacienteValidator.cs`, `CitaValidator.cs`, `ConsultaValidator.cs` | Mod | validar `ConsultorioID` si aplica |

### WebApi (`ConsultorioMedicoWebApi`)
| Archivo | Acción |
|---------|--------|
| `Helpers/Cryptography.cs` | New | PBKDF2 (port de HelpDesk) |
| `Controllers/BaseController.cs` | Mod | (opcional) helper `EmpresaId` tipado `int` |
| `Controllers/RegistroController.cs`, `Services/RegistroService.cs`, `DAL/DbWrapper.Registro.cs` | New | registro transaccional + unicidad |
| `Controllers/AutenticacionController.cs`, `Services/AutenticacionService.cs`, `DAL/DbWrapper.Autenticacion.cs` | Mod | PBKDF2 + `Estatus` + rehash; `ObtenerUsuarioPorUserName` |
| `App_Start/Startup.cs` | Mod | `GrantResourceOwnerCredentials` → PBKDF2 |
| `Controllers/TipoConsultaController.cs`, `Services/TipoConsultaService.cs`, `DAL/DbWrapper.TipoConsulta.cs` | Mod | CRUD completo |
| `DAL/DbWrapper.{Paciente,Cita,Consulta,Anexo,Tratamiento,Paquete,Usuario,DatosInstitucion,DatosFiscales,Historial,Dashboard}.cs` + `Services/*` + `Controllers/*` correspondientes | Mod | inyectar `@ConsultorioID`/`@CompaniaID` del claim; validar pertenencia |
| `Attachment/Web.config` | New | bloquear acceso directo |
| `*.csproj` | Mod | registrar nuevos `.cs` |

### MVC (`ConsultorioMedicoMVC`)
| Archivo | Acción |
|---------|--------|
| `Controllers/RegistroController.cs`, `Services/RegistroService.cs`, `DAL/HttpClientConnection.Registro.cs` | New | captura pública → API |
| `Controllers/TipoConsultaController.cs`, `Services/TipoConsultaService.cs`, `DAL/HttpClientConnection.TipoConsulta.cs` | New/Mod | CRUD |
| `Views/Registro/Index.cshtml` | New | ventana de captura |
| `Views/TipoConsulta/Index.cshtml`, `Views/TipoConsulta/Nuevo.cshtml` | New | CRUD |
| `App_Start/FilterConfig.cs` | Mod | `PublicActions` += `Registro.*` |
| `Views/Shared/_Layout.cshtml` | Mod | branding sidebar/navbar desde tenant |
| `Views/Usuario/Nuevo.cshtml` | Mod | confirmación password; ocultar `CompaniaID` |
| `Controllers/HomeController.cs`, `Views/Home/Autentication.cshtml` | Mod | login sin branding (verificar) |
| `Controllers/DatosInstitucionController.cs`, `Controllers/DatosFiscalesController.cs` | Mod | quitar fallback `: 1`; tenant del claim |
| `Views/Consulta/_RecetaContenido.cshtml` (y receta) | Mod | ya resuelve institución del tenant |
| `*.csproj` | Mod | registrar nuevos `.cs`/`.cshtml` |

### SQL (`sql/`)
| Archivo | Acción |
|---------|--------|
| `migraciones_saas_01_esquema.sql`, `migraciones_saas_02_procedimientos.sql`, `migraciones_saas_03_archivos.sql` | New | migraciones idempotentes (ver §1/§2/§7) |

---

## Data Flow (registro + login)
```
Registro:  View ──> MVC RegistroController ──> WebApi RegistroController ──> DbWrapper.Registro
                                                                               │ BeginTransaction
                                                                               ├─ SaveOrUpdateDatosInstitucion ─┐
                                                                               ├─ sp_nh_SaveDatosFiscal         │ 1 SqlConnection
                                                                               └─ sp_nh_SaveUsuario (PBKDF2)    │ + SqlTransaction
                                                                                 Commit / Rollback ─────────────┘

Login:     View ──> MVC Home.LogIn ──> (1) api/Autenticacion/autenticar (PBKDF2 + rehash)
                                   └─> (2) /token OAuth (Startup → PBKDF2) → claims usuarioId+empresaId
                                         └─> FormsAuth cookie (TokenCookie.EmpresaID)

Tenant:    WebApi = claim empresaId (BaseController) · MVC = TokenCookie.EmpresaID · NUNCA del cliente
```

## Open Questions

- Ninguna bloqueante. Confirmar en apply los nombres reales de las constraints `CitaPaquete` (se resuelven por inspección en la migración). El orden de liberación de los 2 pilotos queda para operación.
