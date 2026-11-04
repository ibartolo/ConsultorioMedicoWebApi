# Tasks: Registro SaaS de Consultorio + Aislamiento Multi-Tenant

> Orden de dependencias: **esquema → SPs → entidades/DTO → auth → registro → plumbing tenant WebApi → MVC/UI → archivos → verificación → ejecución de migraciones (con aprobación)**.
> ⛔ **NO ejecutar ninguna migración SQL sin aprobación explícita del usuario.** Migraciones solo sobre el clon `db_9c7990_consultoriodesitst` (develop/main). Ramas/BD `-dermatologia` y `DB_9C7990_Consultorio` intactas.

---

## Phase 0: Pre-flight, respaldo y ramas

- [x] 0.1 Verificar rama: `git branch --show-current` ∈ {`develop`, `main`}. No trabajar sobre `*-dermatologia`. → rama actual: `develop`. OK.
- [x] 0.2 Verificar BD destino: conexión apunta al clon `db_9c7990_consultoriodesitst` (user `..._admin`), no a `DB_9C7990_Consultorio`. → `cCon` = `sql8010.site4now.net / db_9c7990_consultoriodesitst / db_9c7990_consultoriodesitst_admin`. OK (solo inspección).
- [x] 0.3 Confirmar estado del clon (0 filas en `Paciente/Cita/Consulta/Anexo/PacientePaquete/PagoPaquete/CitaPaquete/Tratamiento/TratamientoPaquete/Paquete/TipoConsulta`; `Usuario` 2 filas; `DatosInstitucion` 1 fila Id=1). → confirmado: 0 filas en las 11 tablas de dominio; `Usuario` 2 filas (`admin` CompaniaID=1, `ivanbartolo` CompaniaID=NULL); `DatosInstitucion` 1 fila Id=1; `DatosFiscales` 0 filas; `CitaPaquete` vacía.
- [ ] 0.4 Respaldo del clon (backup/restore previo a migración). Guardar nombre del respaldo para rollback. → **PENDIENTE de ejecución humana** (no se realizó en este batch).
- [x] 0.5 Registrar en git que no se modifica nada de `-dermatologia` ni `DB_9C7990_Consultorio` (verificación de diff al final). → solo se añadieron archivos nuevos en `sql/`; sin cambios a `-dermatologia` ni a la config de `DB_9C7990_Consultorio`. Verificación de diff final queda en 10.3.

## Phase 1: Migraciones de esquema — `sql/migraciones_saas_01_esquema.sql`

Depende de 0.1–0.5. Todo idempotente (guards `sys.columns`/`sys.indexes`/`sys.foreign_keys`), sin `DROP` de tablas/SPs.

- [x] 1.1 Añadir `ConsultorioID INT NULL` a las 11 tablas (`Paciente`, `Cita`, `Consulta`, `Anexo`, `PacientePaquete`, `PagoPaquete`, `CitaPaquete`, `Tratamiento`, `TratamientoPaquete`, `Paquete`, `TipoConsulta`) con `IF COL_LENGTH(...) IS NULL`.
- [x] 1.2 Backfill: `UPDATE <Tabla> SET ConsultorioID=1 WHERE ConsultorioID IS NULL` (11 tablas).
- [x] 1.3 FK por tabla: `IF NOT EXISTS (sys.foreign_keys name='FK_<Tabla>_ConsultorioID')` → `ADD CONSTRAINT ... REFERENCES DatosInstitucion(Id)`.
- [x] 1.4 `ALTER COLUMN ConsultorioID INT NOT NULL` (las 11, vía `is_nullable=1`).
- [x] 1.5 `Usuario.Password` → `NVARCHAR(250)` (guard por `sys.columns.max_length`).
- [x] 1.6 `Usuario.CompaniaID`: backfill `NULL→1`, luego `NOT NULL`, luego FK → `DatosInstitucion(Id)`. (FK existente `FK_Usuario_DatosInstitucion` detectada: guard por `referenced_object_id` evita duplicar).
- [x] 1.7 Corregir FKs `CitaPaquete`: descubrir nombres reales vía `sys.foreign_keys`+`sys.foreign_key_columns`; `DROP` solo esas FKs malas y `ADD` `FK_CitaPaquete_Cita→Cita(Id)` y `FK_CitaPaquete_PacientePaquete→PacientePaquete(Id)` (guard por `referenced_object_id`). → nombres reales confirmados: `FK_CitaPaquete_Cita` (CitaID→Paciente) y `FK_CitaPaquete_PacientePaquete` (PacientePaqueteID→Paquete).
- [x] 1.8 Unique indexes (`IF NOT EXISTS sys.indexes`): `UX_Usuario_UserName`, `UX_DatosFiscales_RFC`, `UX_DatosFiscales_RazonSocial`, `UX_DatosFiscales_DatosInstitucionID`, `UX_DatosInstitucion_Nombre`, `UX_DatosInstitucion_Email` (opcionales con `WHERE col IS NOT NULL`).
- **Criterios**: script re-ejecutable sin error; `ConsultorioID` NOT NULL+FK en las 11; `CitaPaquete` con FKs corregidas; `Usuario.Password` 250; indexes únicos presentes.
- **Verificación**: inspección vía `INFORMATION_SCHEMA.COLUMNS` / `sys.foreign_keys` / `sys.indexes` en el clon.

## Phase 2: Procedimientos almacenados — `sql/migraciones_saas_02_procedimientos.sql`

Depende de Phase 1. `ALTER PROCEDURE` para existentes; `IF OBJECT_ID(...) IS NULL ... CREATE` para nuevos. Sin `DROP`.

- [x] 2.1 ALTER con `+@ConsultorioID` y filtro/inserción: `sp_nh_GetAllCitas`, `sp_nh_GetCitaById`, `sp_nh_SaveCita`, `sp_nh_DeleteCita`, `GetAllCitaByPacienteID`.
- [x] 2.2 ALTER consultas: `sp_nh_GetAllConsultas`, `sp_nh_GetConsultaById`, `sp_nh_SaveConsulta`, `GetAllConsultaByPacienteID`.
- [x] 2.3 ALTER pacientes: `sp_nh_GetPacienteById`, `sp_nh_SavePaciente`, `sp_nh_DeletePaciente`, `GetAllPacienteByEstatus`.
- [x] 2.4 ALTER usuarios: `sp_nh_GetAllUsuarios`, `sp_nh_GetUsuarioById` (`+@CompaniaID`); `sp_nh_SaveUsuario` sin cambio de firma (servicio asigna `CompaniaID`+hash). (Nota: se amplió `@Password` a `NVARCHAR(250)` en `sp_nh_SaveUsuario` para el hash PBKDF2.)
- [x] 2.5 ALTER `sp_nh_GetDatosFiscales`, `sp_nh_GetDatosFiscalById`, `sp_nh_SaveDatosFiscal`, `sp_nh_DeleteDatosFiscal` (`+@ConsultorioID`; eliminar "Unico" global). (Nota: `@DatosInstitucionID` se renombró a `@ConsultorioID` en `sp_nh_SaveDatosFiscal`; el DbWrapper.Registro de Phase 5 debe pasar `@ConsultorioID`.)
- [x] 2.6 ALTER `GetDatosInstitucion`, `SaveOrUpdateDatosInstitucion` (resolver por `@Id` del tenant). (Nota: inserción devuelve `SCOPE_IDENTITY()`.)
- [x] 2.7 ALTER `sp_nh_GetAnexoById`, `sp_nh_SaveAnexo` (`+@ConsultorioID`) — **el listado inline en `DbWrapper.Anexo.cs` es código C#, se ajusta en un batch posterior**.
- [x] 2.8 ALTER `sp_nh_GetPaqueteById`, `sp_nh_SavePaquete`, `GetAllPaqueteDT`; `sp_nh_GetTratamientoById`, `sp_nh_SaveTratamiento`, `GetAllTratamiento`; `GetAllTipoConsulta`; `sp_nh_GetTratamientosPorPaciente`.
- [x] 2.9 ALTER `sp_nh_GetRecetaByConsulta` (fija `@institucionId` desde claim) y `sp_nh_AutenticacionUsuario` (filtra solo `UserName`, devuelve `Password`+`Estatus`, sin comparar password en SQL).
- [x] 2.10 CREATE nuevos: `sp_nh_GetPaquetesPorPaciente`, `sp_nh_GetCitasPorPacientePaquete` (existían, se alteraron con `@ConsultorioID`); `sp_nh_GetTipoConsultaById`, `sp_nh_SaveTipoConsulta`, `sp_nh_DeleteTipoConsulta`; `sp_nh_UpdateUsuarioPassword`.
- **Criterios**: cada SP filtra por `@ConsultorioID`/`@CompaniaID`; `sp_nh_DeleteCita` = baja lógica `Estatus=0 WHERE Id=@Id AND ConsultorioID=@ConsultorioID`; re-ejecutable sin error.
- **Verificación**: ejecución del script en el clon (aprobación previa) + `SELECT` de prueba por tenant.

> **Hardening Batch 1 (aplicado)** — `sql/migraciones_saas_02_procedimientos.sql` (solo edición; validado con `SET PARSEONLY ON`, sin ejecutar):
> - `Save*` de dominio (`Cita`, `Consulta`, `Paciente`, `DatosFiscal`, `Paquete`, `Tratamiento`, `TipoConsulta`) distinguen inserción vs actualización por `@Id`: `0`→INSERT+`SCOPE_IDENTITY()`; `>0`→`UPDATE ... WHERE Id=@Id AND ConsultorioID=@ConsultorioID`, con `IF @@ROWCOUNT=0 → SELECT NULL; RETURN;`. Nunca insertan con `@Id>0` ajeno.
> - `sp_nh_SaveDatosFiscal` usa filtro `WHERE Id=@Id AND DatosInstitucionID=@ConsultorioID`.
> - `sp_nh_SaveUsuario`: actualiza solo `WHERE Id=@Id AND CompaniaID=@CompaniaID`; preserva `Password` (`ISNULL(@Password, Password)`); inserta solo si `@Id=0`; ya no mueve `CompaniaID` en el `SET`.
> - `sp_nh_SaveAnexo` valida que `PacienteID` (y `ConsultaID` si viene) pertenezcan a `@ConsultorioID`; inválido → `SELECT NULL; RETURN;`.
> - `sp_nh_SaveCita`/`sp_nh_SaveConsulta` validan pertenencia de `PacienteID`, `TratamientoID`, `CitaID` (solo Consulta) y `TipoConsultaID` (solo Consulta); inválido → `SELECT NULL; RETURN;`.
> - `sp_nh_GetAllUsuarios`/`sp_nh_GetUsuarioById` ya NO devuelven `Password` (el login usa `sp_nh_AutenticacionUsuario`, que sí la devuelve para verificación C#).
> - `sp_nh_GetCitasPorPacientePaquete` añade `C.ConsultorioID=@ConsultorioID AND CP.ConsultorioID=@ConsultorioID`; `sp_nh_GetTratamientosPorPaciente` join tenant-safe.
> - ⚠️ **Los servicios (Phase 6) DEBEN tratar el retorno `NULL` como "no encontrado / tenant ajeno" (`IsSuccess=false`).**

## Phase 3: Entidades / DTOs / Validadores — `ConsultorioMedicoEntities`

Depende de Phase 1 (compila sin columna hasta migrar). No tocar patrón de `Paciente` (Factory Method EXCLUIDO).

- [x] 3.1 Añadir `ConsultorioID int` a `Paciente.cs`+`PacienteDTO.cs`, `Cita.cs`+`CitaDTO.cs`, `Consulta.cs`+`ConsultaDTO.cs`, `Anexo.cs`, `Tratamiento.cs`, `Paquete.cs`, `TipoConsulta.cs` (mantener POCO/`BaseObject` actual). → `ConsultorioID` añadido a las 7 entidades base; `PacienteDTO`/`CitaDTO`/`ConsultaDTO` lo heredan (no se duplicó la propiedad).
- [x] 3.2 Crear `Registro/RegistroDTO.cs` (campos de captura) + `Registro/RegistroValidator.cs` (obligatorios `NombreComercial`, `RazonSocial`, `RFC`, `Responsable`, `Calle`, `CorreoContacto`, `Password`+`Confirmacion`; email regex; CP int D5; RFC `[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}`). → `CP` es `int?` (opcional); RFC normalizado `trim().ToUpper()`; mensajes en español; sin chequeos de tenant/BD.
- [x] 3.3 Crear `Catalogos/TipoConsultaDTO.cs` + `Catalogos/TipoConsultaValidator.cs` (`Nombre` requerido; `Duracion`/`Descripcion` opcionales). → creados; DTO hereda de `TipoConsulta` (sin campos extra).
- [x] 3.4 Actualizar `PacienteValidator.cs`, `CitaValidator.cs`, `ConsultaValidator.cs` para validar `ConsultorioID` si aplica. → revisados; **sin cambios**: `ConsultorioID` lo asigna el servidor desde el claim (no es input del cliente), por lo que no se valida en estos validadores compartidos.
- **Criterios**: compilación OK; validadores devuelven mensajes en español.
- **Verificación**: MSBuild proyecto Entities.

## Phase 4: Autenticación / seguridad — WebApi

Depende de Phase 1 (columna `Password` 250), Phase 2.9, Phase 3.

- [x] 4.1 Crear `Helpers/Cryptography.cs` (port PBKDF2: `Rfc2898DeriveBytes` SHA256, 10000 iter, salt 16 B, hash 32 B, formato `PBKDF2$10000$<salt>$<hash>`; `HashPassword`/`Verify` detectando prefijo `PBKDF2$`). → SHA256 explícito (`HashAlgorithmName.SHA256`); `Verify` detecta `PBKDF2$` y, si no lo es, compara texto plano legado; `EsHashPbkdf2` para decidir rehash; comparación en tiempo constante.
- [x] 4.2 Modificar `DAL/DbWrapper.Autenticacion.cs` + `Services/AutenticacionService.cs` + `Controllers/AutenticacionController.cs`: `ObtenerUsuarioPorUserName` (sin filtrar `Estatus`), verificación en C#, rechazo por `Estatus=0`, rehash legado vía `sp_nh_UpdateUsuarioPassword`. → `ObtenerUsuarioPorUserName` (solo `@UserName`), `ActualizarPassword` (rehash), `AutenticarUsuario` reescrito a PBKDF2 (Password=null al devolver). `AutenticacionService` distingue inactivo (`Estatus=0`) de credenciales inválidas. `AutenticacionController` **sin cambios** (firma del servicio intacta).
- [x] 4.3 Modificar `App_Start/Startup.cs` `GrantResourceOwnerCredentials` → usar la misma auth PBKDF2 (no texto plano); emitir claims `usuarioId`+`empresaId`(=CompaniaID). → usa `DbWrapper.AutenticarUsuario` (ahora PBKDF2); claims `usuarioId` y `empresaId` (defensivo `>0`).
- [x] 4.4 Pantalla Usuario: `UsuarioService.GuardarOActualizarUsuario` hashea si password nuevo/no vacío; edición con vacío conserva actual; `CompaniaID` desde claim; `Estatus=1` default. → hashea en alta (`Id==0`) o con password no vacío; edición con vacío pasa `NULL` al SP. `UsuarioValidator` ajustado: password obligatorio solo en alta. `CompaniaID` sigue viniendo del cliente (**pendiente Phase 6**, anotado en código).
- **Criterios**: login por `UserName`; `/token` ya no acepta texto plano; rehash transparente; mensajes genéricos en español.
- **Verificación**: pruebas manuales login (matriz Phase 11).

## Phase 5: Registro público — WebApi + MVC

Depende de Phase 2 (SPs de alta), Phase 3.2, Phase 4.1.

- [x] 5.1 WebApi: `Controllers/RegistroController.cs` (`[AllowAnonymous]`) + `Services/RegistroService.cs` + `DAL/DbWrapper.Registro.cs` — transacción `BeginTransaction`→`SaveOrUpdateDatosInstitucion(@Id=0,Pais="México")`→`sp_nh_SaveDatosFiscal`(1:1)→`sp_nh_SaveUsuario`(hash PBKDF2)→`Commit`; `Rollback` ante excepción. Unicidad pre-check inline (RFC/email/NombreComercial/RazonSocial/UserName) con mensajes específicos.
- [x] 5.2 MVC: `Controllers/RegistroController.cs` + `Services/RegistroService.cs` + `DAL/HttpClientConnection.Registro.cs`.
- [x] 5.3 `Views/Registro/Index.cshtml` (layout `_LayoutPublic`, Bootstrap 5 + jQuery Validate, `serialize()`, hidden inputs, Swal). Confirmación se valida en MVC y se descarta antes de la API.
- [x] 5.4 `App_Start/FilterConfig.cs`: añadir `"Registro.Index"` y `"Registro.Guardar"` a `PublicActions`.
- **Criterios**: alta atómica; RFC normalizado (`trim().ToUpper()`) como `UserName`; rollback total; redirige a `Home/Autentication` (sin auto-login).
- **Verificación**: matriz registro (Phase 11).

## Phase 6: Plumbing de tenant — WebApi (por módulo)

Depende de Phase 2 y Phase 3. Tenant SIEMPRE desde `BaseController.ObtenerEmpresaIdDesdeClaim()`; nunca del body/query/ruta.

- [x] 6.1 `BaseController.cs`: (opcional) helper tipado `int EmpresaId`. Confirmar que `ObtenerEmpresaIdDesdeClaim()` resuelve el claim.
- [x] 6.2 Paciente: `Controllers/PacienteController.cs` + `Services/PacienteService.cs` + `DAL/DbWrapper.Paciente.cs` — inyectar `@ConsultorioID`; validar pertenencia (id ajeno → `IsSuccess=false`).
- [x] 6.3 Cita: controller/service/DbWrapper — calendario filtrado; `Eliminar/{id}` baja lógica acotada.
- [x] 6.4 Consulta + Historial/Expediente: controller/service/DbWrapper (`DbWrapper.Historial`, `DbWrapper.Dashboard` con filtro `ConsultorioID`); `sp_nh_GetRecetaByConsulta` fija `@institucionId` del claim.
- [x] 6.5 Anexo: controller/service/DbWrapper — filtro tenant + validación pertenencia.
- [x] 6.6 Tratamiento/Paquete: conservar CRUD + filtro/inserción por `@ConsultorioID`.
- [x] 6.7 Usuario: listar/filtrar por `@CompaniaID`; alta asigna `CompaniaID` del claim (cliente no elige consultorio).
- [x] 6.8 DatosInstitucion/DatosFiscales: resolver tenant actual; `DatosFiscales` sin "Unico" global.
- [x] 6.9 TipoConsulta CRUD: `Controllers/TipoConsultaController.cs` + `Services/TipoConsultaService.cs` + `DAL/DbWrapper.TipoConsulta.cs` (listar/crear/editar/desactivar soft).
- **Criterios**: ID de otro tenant → `ModelResponse IsSuccess=false` sin datos; suplantación (`empresaId` en body) ignorada.
- **Verificación**: matriz aislamiento/IDOR (Phase 11).

## Phase 7: MVC / UI

Depende de Phase 6.

- [x] 7.1 Asegurar `TokenCookie.EmpresaID` en `BaseController` MVC propagado a servicios/HttpClientConnection.
- [x] 7.2 `Controllers/DatosInstitucionController.cs` y `DatosFiscalesController.cs`: quitar fallback `: 1`; tenant desde cookie.
- [x] 7.3 `Views/Shared/_Layout.cshtml`: reemplazar "Consultorio Médico" hardcodeado (header/navbar-top/sidebar) por `DatosInstitucion.Nombre`/`LogoRuta` del tenant (poblado desde `TokenCookie.EmpresaID`).
- [x] 7.4 `Views/Usuario/Nuevo.cshtml`: confirmación de password; `CompaniaID` no editable.
- [x] 7.5 TipoConsulta CRUD MVC: `Controllers/TipoConsultaController.cs` + `Services/TipoConsultaService.cs` + `DAL/HttpClientConnection.TipoConsulta.cs`; `Views/TipoConsulta/Index.cshtml` + `Nuevo.cshtml` (patrón `Tratamiento`).
- [x] 7.6 `Views/Home/Autentication.cshtml` + `Controllers/HomeController.cs`: verificar login sin branding (sin logo/nombre de institución).
- [x] 7.7 `Views/Consulta/_RecetaContenido.cshtml` (receta): confirmar que resuelve `DatosInstitucion` del tenant (membrete, logo, médico).
- **Criterios**: sidebar/receta con branding del tenant; login neutro; CRUD TipoConsulta funcional en UI.
- **Verificación**: matriz branding/UI (Phase 11).

## Phase 8: Almacenamiento de archivos

Depende de Phase 1 y Phase 6.5.

- [x] 8.1 Crear `Attachment/Web.config`: `<system.web><authorization><deny users="*"/></authorization></system.web>` + restricción `<system.webServer>` (bloquear acceso directo).
- [x] 8.2 Crear `sql/migraciones_saas_03_archivos.sql` (idempotente): `UPDATE DatosInstitucion SET LogoRuta=REPLACE(...) WHERE LogoRuta LIKE '/Attachment/institucion/%'`; `UPDATE Anexo SET Ruta=REPLACE(Ruta,'/Attachment/','/Attachment/1/') WHERE Ruta LIKE '/Attachment/%' AND Ruta NOT LIKE '/Attachment/1/%'`.
- [ ] 8.3 Movimiento físico (manual/apply): `Attachment/institucion/` → `Attachment/1/institucion/`; carpetas huérfanas `Attachment/{num}/` → `Attachment/1/{num}/`. → **PENDIENTE de aprobación (Phase 11). No se movió nada en este batch.**
- [x] 8.4 Anexo modal/API (`_AnexosModal.cshtml`, endpoints): usar ruta `Attachment/{consultorioId}/{pacienteId}/[{consultaId}/]{guid}.ext`; list/upload/download por API con validación tenant; descarga por proxy MVC.
- **Criterios**: logo bajo `Attachment/1/institucion/`; acceso directo a archivo denegado; rutas actualizadas.
- **Verificación**: matriz anexos (Phase 11).

## Phase 9: Registro de archivos en `.csproj`

Depende de fases previas.

- [x] 9.1 Registrar nuevos `.cs` en `ConsultorioMedicoWebApi.csproj` (Registro, Cryptography, DbWrapper.Registro, etc.). → ya registrados en batches previos; en este batch solo se añadió `Attachment\Web.config` (Content).
- [x] 9.2 Registrar nuevos `.cs`/`.cshtml` en `ConsultorioMedicoMVC.csproj` (Registro, TipoConsulta, vistas).
- **Verificación**: compilación sin `File not found` / missing members.

## Phase 10: Verificación de compilación

Depende de fases 1–9.

- [ ] 10.1 MSBuild Debug solución completa → exit 0.
- [ ] 10.2 `aspnet_compiler -p ConsultorioMedicoMVC` → exit 0 (precompila vistas Razor).
- [ ] 10.3 Revisar `git diff` para confirmar que NO se tocó nada de `-dermatologia` ni `DB_9C7990_Consultorio`.
- **Criterios**: ambos comandos exit 0; mensajes en español; sin errores Razor C#5.

## Phase 11: Ejecución de migraciones (con aprobación) y pruebas manuales

⛔ **Gate de aprobación**: antes de ejecutar `01/02/03` en el clon, pedir aprobación explícita del usuario. Orden: `01_esquema` → `02_procedimientos` → `03_archivos` (tras mover archivos físicos).

- [ ] 11.1 Ejecutar `migraciones_saas_01_esquema.sql` en el clon (previa aprobación). Verificar backfill a 1, `ivanbartolo.CompaniaID=1`, `DatosFiscales` consultorio 1 vacío.
- [ ] 11.2 Ejecutar `migraciones_saas_02_procedimientos.sql` (previa aprobación).
- [ ] 11.3 Ejecutar `migraciones_saas_03_archivos.sql` (previa aprobación) tras mover archivos físicos.

### Matriz de pruebas manuales

| # | Escenario | Resultado esperado |
|---|-----------|--------------------|
| M1 | Registro consultorio válido | Se crean `DatosInstitucion`+1 `DatosFiscales`+`Usuario` admin (`UserName`=RFC mayúsculas); Swal éxito → login. |
| M2 | Registro RFC/email/NombreComercial/RazonSocial/UserName duplicado | Rechazo con mensaje específico; nada creado. |
| M3 | Registro con fallo parcial (forzar error en fiscal/usuario) | Rollback total; sin consultorio parcial. |
| M4 | RFC inválido / CP no D5 / email inválido / passwords no coinciden | Rechazo por validación (jQuery + servidor). |
| M5 | Login con hash PBKDF2 correcto | Sesión + token establecidos; claims `usuarioId`/`empresaId`. |
| M6 | Login usuario legado (texto plano) | Acepta y rehashea a `PBKDF2$...` transparentemente. |
| M7 | Login usuario inactivo (`Estatus=0`) | Rechazo con mensaje claro. |
| M8 | `/token` OAuth con credenciales | Verifica PBKDF2 (no texto plano). |
| M9 | Listado Paciente/Cita/Consulta/Anexo/Tratamiento/Paquete/TipoConsulta | Solo registros del tenant del usuario. |
| M10 | Consultar/editar/eliminar ID de otro consultorio (IDOR) | `ModelResponse IsSuccess=false` sin datos. |
| M11 | Enviar `empresaId`/`ConsultorioID`=3 en body | Backend lo ignora; usa tenant del claim. |
| M12 | CRUD TipoConsulta (crear/editar/desactivar) | Opera sobre catálogo del tenant; `Nombre` requerido; soft delete `Estatus=0`. |
| M13 | Alta de Usuario | `CompaniaID` asignado automáticamente del claim; cliente no elige consultorio. |
| M14 | DatosInstitucion/DatosFiscales | Resuelve tenant actual; edición solo del propio consultorio. |
| M15 | Sidebar + receta | Muestran nombre/logo/médico del tenant actual. |
| M16 | Login | Sin logo/nombre de institución. |
| M17 | Anexos subir (PDF≤5MB, imagen≤2MB) / listar / descargar | Ruta `Attachment/{consultorioId}/{pacienteId}/...`; acceso directo a URL denegado. |
| M18 | FKs `CitaPaquete` | `CitaID→Cita.Id`, `PacientePaqueteID→PacientePaquete.Id`. |
| M19 | Revisión de diff | Cero cambios en `-dermatologia` / `DB_9C7990_Consultorio`. |

---

## Rollback y checkpoints

- **Checkpoints**: (C1) tras Phase 1 aprobada y aplicada; (C2) tras compilación OK (Phase 10); (C3) tras pruebas manuales verdes (Phase 11). Confirmar con el usuario antes de avanzar de C1.
- **Rollback de código**: todo es aditivo → `git checkout` de archivos nuevos/modificados (revertible por versionado).
- **Rollback de datos/migración**: restaurar el respaldo del clon (0.4). No hay script destructivo; `DROP` limitado a FKs incorrectas de `CitaPaquete`.
- **Rollback de auth**: el login conserva fallback a texto plano hasta confirmar el rehash → no bloquea el acceso.

## Fuera de alcance (no implementar)

Roles/permisos, trial/vigencia/licencia/pagos/límites, superadmin, seeds de catálogos, refactor Factory Method de `Paciente`, ramas/BD `-dermatologia`, BD productiva SaaS.

---

## Batch 5.1 — Fixes from sdd-verify (aplicados, SIN ejecutar SQL)

Correcciones derivadas de `verify-report.md` (PASS WITH WARNINGS). Build MSBuild Debug + aspnet_compiler → exit 0. No se ejecutó SQL ni se movieron archivos.

- **WARNING #1 — trim en registro**: `WebApi/Services/RegistroService.cs` normaliza (trim) `NombreComercial`, `RazonSocial`, `Responsable`, `Calle`, `NoExterior`, `NoInterior`, `Colonia`, `Municipio`, `Estado`, `Telefono`, `CorreoContacto` antes de validar/insertar. El `DbWrapper.Registro` persiste los mismos valores DTO normalizados (su pre-check ya recortaba). La contraseña NO se recorta.
- **SUGGESTION #1 — política de contraseña**: `RegistroValidator` exige longitud ≥ 6 y rechaza solo-espacios; conserva la comparación de confirmación. Sin complejidad adicional.
- **WARNING #2 — filtro Estatus de `GetAllTipoConsulta`**: `migraciones_saas_02_procedimientos.sql` añade `@SoloActivos BIT = 1` con `WHERE (@SoloActivos = 0 OR Estatus = 1) AND ConsultorioID = @ConsultorioID`. `DbWrapper.TipoConsulta` acepta el flag; WebApi expone `List` (activos) y `ListAdmin` (todos); MVC `TipoConsultaService`/`HttpClientConnection` usan `ListAdmin` para el `Index` (badge Estatus) y `List` (activos) para los dropdowns de Consulta/Historial.
- **SUGGESTION #4 — enlace login → registro**: `Views/Home/Autentication.cshtml` enlaza "Registrar consultorio" a `Registro/Index` (se elimina el modal stub "Solicitar acceso"). Login sigue neutro.
- **SUGGESTION #5 — id decorativo**: `Views/DatosInstitucion/Index.cshtml` deja de enviar `?id=` a `Logo` y `{ id }` a `Consultar` (rutas tenant-scoped por cookie).
- **SUGGESTION #7 — SP legado**: `sql/sp_nh_AutenticacionUsuario.sql` marcado DEPRECADO, no-ejecutable (solo PRINT); redirige a `migraciones_saas_02_procedimientos.sql`.
- **NOTA — `sql/reset_datos_consultorio.sql`**: sin trackear y FUERA del cambio; NO se borra ni modifica y NO debe ejecutarse para SaaS.
- **Sin cambios** (documentado): validadores CP/teléfono (SUGGESTION #2/#3), elección SHA256/PBKDF2, `CREATE OR ALTER` (SUGGESTION #6).
