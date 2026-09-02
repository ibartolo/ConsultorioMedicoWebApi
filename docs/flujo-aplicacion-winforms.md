# Flujo de la aplicación WinForms "Consultorio Médico"

> **Documento de referencia** generado por descompilación (ILDASM) del build ClickOnce instalado
> (`ConsultorioMedico.exe`, fechado 20/05/2025) y contrastado contra el `script.sql` de la BD real
> `DB_9C7990_Consultorio`.
>
> Este documento describe **lo que la aplicación hace hoy**, acción por acción: ventanas, botones,
> métodos de negocio y procedimientos/consultas que se ejecutan, con sus campos.

---

## 1. Resumen ejecutivo

| Dato | Valor |
|---|---|
| Tecnología | .NET Framework 4.0, **x86**, WinForms |
| UI | DevComponents DotNetBar 11.2 (SuperGrid, Schedule, Ribbon) |
| Acceso a datos | **NHibernate 4.1 + FluentNHibernate 2.0** (CRUD) **+ ADO.NET crudo** (consultas puntuales) |
| Reportes | Microsoft ReportViewer (RDLC) |
| Base de datos | SQL Server (Smarterasp.net), `DB_9C7990_Consultorio` |
| Conexión | `cCon` en `ConsultorioMedico.exe.config` (credenciales en texto plano) |

**Punto clave del acceso a datos:** la app **no** usa los procedimientos `SaveOrUpdate*` ni `GetPacienteById`
como SP. El CRUD (guardar, eliminar, leer-entidad) se hace por **NHibernate** contra las tablas directamente.
Los **stored procedures** solo se invocan para ~15 consultas de listado/filtrado (métodos `...DT`), y hay
algo de SQL inline.

---

## 2. Arquitectura (3 capas + entidades)

```
ConsultorioMedico.exe                    (UI: ventanas, botones, grids, calendario)
        │  llama a
        ▼
Desi.ConsultorioMedico.Data.BL.dll       (Negocio: XxxBL — validación + orquestación)
        │  llama a
        ▼
Desi.ConsultorioMedico.Data.DA.dll       (Datos: XxxDA — NHibernate + ADO.NET)
Desi.ConsultorioMedico.Data.VO.dll       (Entidades + mapas FluentNHibernate)
```

**Namespaces en BL/DA/VO:**
- `Catalogos.*` → Paciente, Cita, Consulta, Anexo, Paquete, Tratamiento, TipoConsulta, Usuario, DatosInstitucion, DatosFiscales.
- `Asociaciones.*` → CitaPaquete, PacientePaquete, PagoPaquete, TratamientoPaquete (tablas puente).
- `Seguridad.*` → Autenticacion.
- `Utilidades.*` → manejo de archivos (anexos).

**Clases base de datos (DA):** `BaseDataAccess` mantiene la sesión NHibernate (`m_session`) y la
conexión ADO.NET (`cCon`).

---

## 3. Modelo de datos (tablas de `script.sql`)

| Tabla | Campos principales | Auditoría |
|---|---|---|
| `Paciente` | Id, Nombre, ApellidoPaterno/Materno, Email, FechaAlta, TelefonoCel, Comentarios, Estatus, Edad, Sexo, FechaNacimiento | **CreatedBY/DT, UpdatedBY/DT** (nvarchar) |
| `Cita` | Id, Estatus, FechaCita, PacienteID, Comentarios, TratamientoID, Estado | — |
| `Consulta` | Id, Estatus, FechaConsulta, PacienteID, TratamientoID, CitaID, Descripcion, TipoConsultaID, DescripcionReceta, Comentarios, FechaProximaConsulta | — |
| `Anexo` | Id, Estatus, Fecha, Comentarios, PacienteID, Nombre, Ruta, ConsultaID | — |
| `Paquete` | Id, Nombre, Descripcion, FechaCreacion, Estatus, Costo | — |
| `Tratamiento` | Id, Nombre, Duracion, Descripcion, Estatus | — |
| `TipoConsulta` | Id, Nombre, Duracion, Descripcion, Estatus | — |
| `PacientePaquete` | Id, PacienteID, PaqueteID | — |
| `PagoPaquete` | Id, Pago, FechaPago, PacientePaqueteID | — |
| `CitaPaquete` | Id, CitaID, PacientePaqueteID | — |
| `TratamientoPaquete` | Id, PaqueteID, TratamientoID | — |
| `DatosInstitucion` | Id, Nombre, NoTrabajadores, Teléfonos, Email, dirección, CP, País | — |
| `DatosFiscales` | Id, RazonSocial, RFC, Teléfonos, Email, dirección, DatosInstitucionID | — |
| `Usuario` | Id, UserName, Password, Estatus, **CompaniaID** | — |

> **Nota de tenencia:** la única columna de "empresa" es `Usuario.CompaniaID` (FK → `DatosInstitucion`).
> Las tablas de negocio **no** tienen columna de empresa. El SP `GetAllPacientes` filtra por empresa
> usando `PermisosPorCompania(@CompaniaID)` + `Paciente.CreatedBY`, pero **la app actual no lo usa**
> (ver §9).

---

## 4. Autenticación (login)

**Ventana:** `Autenticacion` (`ConsultorioMedico.Autenticacion`)

- Controles: `txtUsuario`, `txtPassword` (label `labelUsuario`).
- Evento de carga: `Autenticacion_Load`.

**Flujo de login:**

```
[Botón entrar] → AutentificacionBL.AutenticacionUsuario(userName, password)
        → AutenticacionDA.AutenticacionUsuario(userName, password)
        → NHibernate: session.CreateCriteria(Usuario)
                        .Add(Restrictions.Eq("UserName", userName))
                        .Add(Restrictions.Eq("Password", password))
                        .List()
        → retorna Usuario (o null si no coincide)
```

**Hallazgos críticos:**
1. **No** usa el SP `Autentication`.
2. Compara `UserName` + `Password` en **texto plano**.
3. **No** valida `Estatus` (un usuario inactivo con password correcto entraría).
4. La entidad `Usuario` de la app **no** mapea `CompaniaID` → no se resuelve la empresa al loguear.

---

## 5. Navegación / menú principal

El formulario principal (ribbon) expone botones que abren los catálogos:

| Botón | Abre |
|---|---|
| `btnPacientes` | `CatalogoPacientes` / `VPacientes` |
| `btnCitas` | `AltaCita` / agenda |
| `btnConsultas` | `NuevaConsulta` / `AltaConsulta` |
| `btnPaquetes` | `CatalogoPaquetes` |
| `btnTratamientos` | `VCatalogoDeTratamientos` |
| `btnCatalogoUsuarios` / `mtUsuarios` | `CatalogoUsuarios` |

---

## 6. Flujo por ventana

### 6.1 Catálogo de Pacientes — `CatalogoPacientes`

**Propósito:** listar y localizar pacientes.

- Controles: `dgvPacientes` (grid), `cbFiltradoPor` (combo de filtro), `btnNuevoPaciente`, `btnDetallePaciente`.
- `CatalogoPacientes_Load` → carga el grid de pacientes.
- `cbFiltradoPor_SelectedIndexChanged` → filtra el listado.
- `dgvPacientes_CellDoubleClick` → abre `VDatosDelPaciente` (detalle).
- `btnNuevoPaciente_Click` → abre `NuevoPaciente`.

**Acceso a datos:**
- `PacienteBL.GetAllPaciente()` → NHibernate (lista todas las entidades Paciente).
- `PacienteBL.GetAllPacienteDT()` → DataTable.
- `PacienteBL.GetAllPacienteByEstatus` vía `PacienteBL::GetAllPacienteDT` **→ SP `GetAllPacienteByEstatus`** (solo `Estatus = 1`, `ORDER BY Id DESC`).

> Nota: el SP `GetAllPacientes` (el que filtra por `@CompaniaID`) **no** se invoca desde esta ventana.

---

### 6.2 Búsqueda de pacientes — `BuscarPaciente`

- Controles: `txtBuscar` / autocomplete; `BuscarPaciente_Load`.
- Acceso: `PacienteBL.LoadAutoComplete()` → NHibernate (sugerencias de autocompletado por nombre).

---

### 6.3 Nuevo / Editar Paciente — `NuevoPaciente`

**Propósito:** alta o edición de un paciente.

- Controles (campos): Nombre, ApellidoPaterno, ApellidoMaterno, Email, TeléfonoCel, Comentarios, Sexo, FechaNacimiento.
- `NuevoPaciente_Load` → si viene un paciente seleccionado, carga sus datos (`PacienteBL.GetPacienteById` → NHibernate).

**Guardar:**

```
btnGuardar → PacienteBL.SavePaciente(paciente)
        → PacienteDA.SavePaciente
        → NHibernate session.SaveOrUpdate(Paciente)
```

**Hallazgo:** la entidad Paciente **no** setea `CreatedBY/DT` ni `UpdatedBY/DT`. Esas columnas quedan en `NULL`
cuando se crea desde la app (motivo por el cual el filtro por empresa del SP `GetAllPacientes` no funciona).

---

### 6.4 Detalle de Paciente — `VDatosDelPaciente`

**Propósito:** ficha completa del paciente (datos + citas + consultas + anexos).

- `VDatosDelPaciente_Load` carga en paralelo:

| Sección | Método BL | DA | Acceso |
|---|---|---|---|
| Datos del paciente | `PacienteBL.GetPacienteById` | NHibernate | entidad Paciente |
| Citas | `CitaBL.GetAllCitaByPacienteIdDT` | **SP `GetAllCitaByPacienteID`** | Cita |
| Consultas | `ConsultaBL.GetAllConsultaByPacienteIdDT` | **SP `GetAllConsultaByPacienteID`** | Consulta |
| Anexos | `AnexoBL.GetAllAnexoByPacienteIdDT` | **SP `GetAllAnexoByPacienteID`** | Anexo |

- Botones:
  - `btnNuevaCita_Click` → `AltaCita`.
  - `btnNuevaConsulta_Click` → `NuevaConsulta`.
  - `btnAgregarAnexo_Click` → `AnexosdeConsulta`.
  - `btnVisualizarAnexo_Click` → `VisualizacionAnexo`.
  - `btnDetalleConsulta_Click` → detalle de consulta.
  - `btnEliminarConsulta_Click` → `ConsultaBL.DeleteConsulta` (NHibernate).

---

### 6.5 Citas — `AltaCita` (alta) y `DetalleCita` (detalle)

**`AltaCita`** (crear/editar cita):
- Controles: `cbPaciente`, `cbtratamiento`, `txtFechaCita`, `dtHoraCita`, `txtComentario`.
- `AltaCita_Load`:
  - `cargarComboPaciente()` → `PacienteBL.GetAllPaciente` (NHibernate).
  - `cargarComboTratamiento()` → `TratamientoBL.GetAllTratamiento` (**SQL inline**, ver §8).
- Guardar: `CitaBL.SaveCita` → `CitaDA.SaveCita` → NHibernate `SaveOrUpdate(Cita)`.

**`DetalleCita`** (ver cita):
- `DetalleCita_Load` → `CitaBL.GetCitaByIdForDetalleCita` → **SQL inline** (ver §8).
- `dgvCita_CellDoubleClick` → abre `DetalleCita`.

**Consulta de citas por período / fecha:**
- `CitaBL.GetCitasByPeriodoFecha` → **SQL inline**.
- `CitaBL.GetCitabyFecheCitaDT` → **SP `GetCitaByFechaCita`**.
- `CitaBL.GetAllCitaByTratamientoIdDT` → **SP `GetAllCitaByTratamientoId`**.
- `CitaBL.GetAllCitaByTratamientoIdDTForPublicidad` → **SP `GetAllCitaByTratamientoIdForPublicidad`** (agrega `Email` del paciente, para publicidad).
- `CitaBL.GetCitaByPacienteIdandFecha` → **SP `GetCitabyPacienteIDandFechaActual`** (cita del día).

**Agendas (calendario):**
- `Agenda.BandejaHorarios` y `AgendaST.BandejaHorariosST` → vistas de calendario (DevComponents.Schedule)
  que cargan citas/consultas por período.

---

### 6.6 Consultas — `AltaConsulta` / `NuevaConsulta` / `AnexosdeConsulta`

**`AltaConsulta` / `NuevaConsulta`** (crear/editar consulta):
- Controles: `cbPaciente`, `cbTipoConsulta`, `cbtratamiento`, `txtDescripcionConsulta`, `txtDescripcionReceta`,
  `txtConsulta`, `dtFechaConsulta`, `dtHoraConsulta`, `dtProximaConsulta`.
- `AltaConsulta_Load`:
  - `cargarComboPaciente` (NHibernate).
  - `cargarComboConsultas` → `TipoConsultaBL.GetAllTipoConsulta` → **SP `GetAllTipoConsulta`**.
  - `cargarComboTratamiento` (SQL inline).
- `cbTipoConsulta_SelectedIndexChanged` → recarga duración/tipo.
- Guardar: `ConsultaBL.SaveConsulta` → NHibernate `SaveOrUpdate(Consulta)`.

**Consultas por fecha/tipo:**
- `ConsultaBL.GetAllConsultaByTipoConsultaIdDT` → **SP `GetAllConsultaByTipoConsultaId`**.
- `ConsultaBL.GetConsultaByPeriodoFecha` / `GetConsultaByFechaConsultaDT` → **SP `GetConsultaByFechaConsulta`**.
- `ConsultaBL.GetConsultaMayor` → **SP `GetConsultaMayor`** (última consulta).

**`AnexosdeConsulta`** (anexos de una consulta):
- Controles: `txtNombreAnexo`, `txtDescripAnexo`, `txtBuscarAnexo` (búsqueda), grid de anexos.
- `txtBuscarAnexo_TextChanged` / `_KeyPress` → filtra anexos.
- `btnAgregarAnexo_Click` → `AnexoBL.SaveAnexo` → NHibernate + `UtilidadesBL.SaveByteArrayToFile` (guarda archivo en disco).
- Lectura: `AnexoBL.GetAnexoByConsultaId`, `GetAnexoByPacienteIdandConsultaId`, `GetAnexoById`, `GetAnexoByIdFromDelete` → NHibernate.
- `VisualizacionAnexo` → muestra/abre el anexo (`UtilidadesBL.GetFileType`).

---

### 6.7 Paquetes — `CatalogoPaquetes`

- Controles: `dgvPaquetes`, `btnNuevoPaquete`, `mtiPaquete`.
- `CatalogoPaquetes_Load` → `PaqueteBL.GetAllPaqueteDT` → **SP `GetAllPaqueteDT`** (lista con `Costo`, `Estatus = 1`).
- `dgvPaquetes_CellDoubleClick` → detalle de paquete (tratamientos del paquete).
- `CargarComboTratamientoInPaquete` → `TratamientoPaqueteBL.GetAllTratamiendoByPaqueteIdDT` → **SP `GetAllTratamiendoByPaqueteID`**.
- Guardar paquete: `PaqueteBL.SavePaquete` → NHibernate.

**Pagos de paquete:**
- `PagoPaqueteBL.GetAllPagoPaqueteByPacientePaqueteIdDT` → **SP `GetAllPagoPaqueteByPacientePaqueteIdDT`** (Precio, Pagado, Adeuda).
- `PagoPaqueteBL.SavePagoPaquete` → NHibernate.

---

### 6.8 Tratamientos — `VCatalogoDeTratamientos`

- Controles: `dgvTratamientos`, `bntGuardarTratamiento`, `btnCancelarTratamiento`, `txtComentararioTratamiento`, `cbTratamientos`.
- `VCatalogoDeTratamientos_Load` → `TratamientoBL.GetAllTratamiento` → **SP `GetAllTratamiento`** o SQL inline.
- `bntGuardarTratamiento_Click` → `TratamientoBL.SaveTratamiento` → NHibernate.
- `btnCancelarTratamiento_Click` → cancela edición.
- `dgvTratamientos_CellDoubleClick` → carga en campos para editar.

---

### 6.9 Usuarios — `CatalogoUsuarios`

- Controles: `mtUsuarios`, `btnCatalogoUsuarios`.
- Acceso: `UsuarioBL.GetUsuarioById`, `GetAllUsuario`, `SaveUsuario` → NHibernate (entidad `Usuario`).
- **Sin filtro por empresa** (no usa `CompaniaID`).

---

### 6.10 Institución / Datos fiscales

- `DatosInstitucionBL.GetAllDatosInstitucion` / `SaveDatosInstitucion` → NHibernate.
- `DatosFiscalesBL.GetAllDatosFiscales` / `GetDatosFiscalesById` / `SaveDatosFiscales` → NHibernate.
- (Los SP `GetDatosInstitucion` / `SaveOrUpdateDatosInstitucion` **no** se invocan desde la UI.)

---

### 6.11 Historial — `CatalogoHistorial`

- Agrupa citas + consultas + paquetes de un paciente (usa campos estáticos `_paciente`, `_pacientepaquete`).
- Lecturas por NHibernate (CitaBL/ConsultaBL/PacientePaqueteBL/PagoPaqueteBL).

---

### 6.12 Reporte de receta — `GetRecetaByConsulta` (RDLC)

- Reporte (ReportViewer) con TableAdapter `GetRecetaByConsultaTableAdapter` → **SP `GetRecetaByConsulta`**.
- Campos que devuelve: `Paciente`, `FechaConsulta`, `Consulta`, `Medicamento` (=DescripcionReceta), `TipoConsulta`, `ProximaConsulta`.

---

## 7. Catálogo de Stored Procedures (uso real)

Leyenda: ✅ = invocado por la app · ❌ = existe en BD pero **no** lo usa la app · 🔶 = solo vía reporte/DataSet.

| SP | Params | Campos devueltos | Uso |
|---|---|---|---|
| `Autentication` | @UserName, @Password | * (Usuario) | ❌ (login por NHibernate) |
| `GetAllAnexoByPacienteID` | @IdPaciente | A.Id, Estatus, Fecha, Comentarios, Nombre, Ruta | ✅ |
| `GetAllCitaByPacienteID` | @IdPaciente | C.Id, FechaCita, Comentarios, T.Nombre, Estatus, PacienteID, TratamientoID, Estado | ✅ |
| `GetAllCitaByTratamientoId` | @TratamientoId | P.Id, NombreCompleto, TelefonoCel, CitaId, UltimaCita, ComentariosCita, Tratamiento, Estado, EstadoCita | ✅ |
| `GetAllCitaByTratamientoIdForPublicidad` | @TratamientoId | (igual + Email) | ✅ |
| `GetAllCitaTratamiento` | — | C.*, Nombre, Apellidos, TratamientoNombre, Duracion | ❌ |
| `GetAllConsultaByPacienteID` | @IdPaciente | CO.Id, Estatus, FechaConsulta, Descripcion, PacienteID, TratamientoID, CitaID, TipoConsultaID, TC.Nombre, DescripcionReceta | ✅ |
| `GetAllConsultaByTipoConsultaId` | @TipoConsultaId | P.Id, NombreCompleto, TelefonoCel, ConsultaId, UltimaConsulta, DescripcionConsulta, Consulta | ✅ |
| `GetAllConsultaCalendario` | — | C.*, Nombre, Apellidos, ConsultaNombre, Duracion | ❌ |
| `GetAllPacienteByEstatus` | — | P.* (16 campos, Edad calculada) | ✅ |
| `GetAllPacientes` | **@CompaniaID** | * (filtro por PermisosPorCompania) | ❌ (código muerto) |
| `GetAllPagoPaqueteByPacientePaqueteIdDT` | @pacientepadoID | Precio, Pagado, Adeuda | ✅ |
| `GetAllPaqueteDT` | — | P.Id, Nombre, Descripcion, FechaCreacion, Estatus, Costo | ✅ |
| `GetAllPaquetes` | — | * | ❌ |
| `GetAllTipoConsulta` | — | * | ✅ |
| `GetAllTratamiendoByPaqueteID` | @paqueteID | TratamientoID AS ID, Nombre | ✅ |
| `GetAllTratamiento` | — | * | ✅ |
| `GetCitaByFechaCita` | @fechaCita | C.Id, Estatus, FechaCita, PacienteId, Comentarios, TratamientoId | ✅ |
| `GetCitaByPaciente` | @PacienteID | C.*, Nombre, Apellidos, TratamientoNombre | ❌ |
| `GetCitabyPacienteIDandFechaActual` | @IdPaciente | C.Id, Estatus, FechaCita, PacienteID, Comentarios, TratamientoID | ✅ |
| `GetCitaTratamientoById` | @Id | C.*, PacienteID, Nombre, Apellidos, TratamientoNombre | ❌ |
| `GetConsultaByFechaConsulta` | @fechaConsulta | C.Id, Estatus, FechaConsulta, PacienteId, Descripcion, TratamientoId, CitaID, TipoConsultaID, FechaFin | ✅ |
| `GetConsultaByPaciente` | @PacienteID | C.*, TipoConsultaNombre | ❌ |
| `GetConsultaCalendarioByIdConsulta` | @Id | C.*, Nombre, Apellidos, ConsultaNombre, Duracion | ❌ |
| `GetConsultaMayor` | — | CO.Id, Estatus, FechaConsulta, Descripcion, PacienteID | ✅ |
| `GetDatosInstitucion` | @Id | * | ❌ |
| `GetPacienteByFechaNacimiento` | — | Nombre, Apellidos, Edad, Telefono, CorreoElectronico | ✅ |
| `GetPacienteById` | @Id, **@CompaniaID** | * (filtro comentado) | ❌ (método NHib) |
| `GetPacientesFiltrados` | @Nombre, @APaterno, @AMaterno, **@CompaniaID** | * (filtro comentado) | ❌ |
| `GetPaqueteById` | @Id | * | ❌ (método NHib) |
| `GetRecetaByConsulta` | @consultaID | Paciente, FechaConsulta, Consulta, Medicamento, TipoConsulta, ProximaConsulta | 🔶 (reporte) |
| `GetTipoConsultaById` | @Id | * | ❌ (método NHib) |
| `GetTratamientoById` | @Id | * | ❌ (método NHib) |
| `GetTratamientoPorPauqte` | @Id | * | ❌ |
| `SaveOrUpdateCita` | 7 params | Id (SELECT) | ❌ (NHib) |
| `SaveOrUpdateConsulta` | 9 params | Id (SELECT) | ❌ (NHib) |
| `SaveOrUpdateDatosInstitucion` | 14 params | Id | ❌ (NHib) |
| `SaveOrUpdatePaciente` | 12 params | Id | ❌ (NHib) |
| `SaveOrUpdatePaquete` | 6 params | Id | ❌ (NHib) |
| `SaveOrUpdateTratamiento` | 5 params | Id | ❌ (NHib) |
| `SaveOrUpdateTratamientoPaquete` | 3 params | Id | ❌ (NHib) |
| `SaveOrUpdateUsuario` | 5 params | Id | ❌ (NHib) |

---

## 8. SQL inline y acceso NHibernate

**SQL inline (ADO.NET crudo en DA):**

1. `Select * from Tratamiento WHERE ESTATUS = 1 Order by ...` → `TratamientoBL.GetAllTratamiento` / combo tratamiento.
2. `Select C.Id, C.Estatus, C.FechaCita, C.PacienteId, ...` → `CitaBL.GetCitaByIdForDetalleCita` / citas por período.
3. `Select P.Id, P.Nombre, PP.PacienteID, PP.Id as Paci...` → `PacientePaqueteBL` / paciente-paquete.

**NHibernate (SaveOrUpdate / CreateCriteria / Get<T>):** usado para **todo** el CRUD:
`SavePaciente`, `SaveCita`, `SaveConsulta`, `SaveAnexo`, `SavePaquete`, `SaveTratamiento`, `SaveUsuario`,
`SavePacientePaquete`, `SavePagoPaquete`, `SaveDatosInstitucion`, `SaveDatosFiscales`, los `Delete*`,
y las lecturas `GetXById` / `GetAllX` de entidades.

---

## 9. Hallazgos importantes para la migración

1. **Multi-tenancy dormido.** El mecanismo de "empresa" (`Usuario.CompaniaID`, `PermisosPorCompania`,
   `GetAllPacientes @CompaniaID`) **no se usa** en la app actual. La app es single-tenant de facto.
2. **`CreatedBY/DT` y `UpdatedBY/DT` no se escriben.** El CRUD por NHibernate no sella la auditoría, por lo
   que el filtro por empresa (que depende de `CreatedBY`) no funcionaría aunque se invocara.
3. **Contraseñas en texto plano.** Tanto en `Usuario.Password` (comparado vía NHibernate sin hash) como en
   las credenciales del `.config` (connection string + email Gmail).
4. **Login no valida `Estatus`.** El `CreateCriteria` solo filtra por `UserName` + `Password`.
5. **La app conecta a dos BDs.** `cCon` → `DB_9C7990_Consultorio` (actual) y un `Settings` generado que aún
   referencia `DB_9BA6B8_Consultorio` (vieja).
6. **Bugs heredados en la BD** (de `script.sql`): FKs de `CitaPaquete` apuntan a tablas equivocadas,
   `FechaProximaConsulta` se setea con `@FechaConsulta` en `SaveOrUpdateConsulta`, `Estatus` se setea con
   `@Estado` en `SaveOrUpdateCita`, fechas de auditoría como `nvarchar`, typo `Deleacion`, `Tratamiento` en `varchar`.

---

## 10. Lista completa de ventanas (formularios)

| Form | Namespace |
|---|---|
| Login | `ConsultorioMedico.Autenticacion` |
| Catálogo de pacientes | `ConsultorioMedico.Catalogos.CatalogoPacientes` |
| Búsqueda de pacientes | `ConsultorioMedico.Catalogos.BuscarPaciente` |
| Nuevo/editar paciente | `ConsultorioMedico.Catalogos.NuevoPaciente` |
| Detalle de paciente | `ConsultorioMedico.NuevaVersion.VDatosDelPaciente` |
| Vista principal de pacientes | `ConsultorioMedico.NuevaVersion.VPacientes` |
| Alta de cita | `ConsultorioMedico.Catalogos.AltaCita` |
| Detalle de cita | `ConsultorioMedico.Catalogos.DetalleCita` |
| Agenda horarios | `ConsultorioMedico.Catalogos.Agenda.BandejaHorarios` |
| Agenda horarios (ST) | `ConsultorioMedico.Catalogos.AgendaST.BandejaHorariosST` |
| Nueva consulta | `ConsultorioMedico.Catalogos.NuevaConsulta` |
| Alta consulta | `ConsultorioMedico.Catalogos.AltaConsulta` |
| Anexos de consulta | `ConsultorioMedico.Catalogos.AnexosdeConsulta` |
| Visualizar anexo | `ConsultorioMedico.Catalogos.VisualizacionAnexo` |
| Catálogo de paquetes | `ConsultorioMedico.Catalogos.CatalogoPaquetes` |
| Catálogo de usuarios | `ConsultorioMedico.Catalogos.CatalogoUsuarios` |
| Catálogo de tratamientos | `ConsultorioMedico.NuevaVersion.VCatalogoDeTratamientos` |
| Historial | `ConsultorioMedico.Catalogos.CatalogoHistorial` |
