# Verificación post-migración — saas-registro-aislamiento

**Fecha:** 2026-11-04  
**BD:** `db_9c7990_consultoriodesitst` (clon SaaS)  
**Intocadas:** `DB_9C7990_Consultorio` y ramas `-dermatologia`

---

## Ejecución de migraciones

| Script | Resultado |
|---|---|
| `sql/migraciones_saas_01_esquema.sql` | Primera ejecución falló al crear los índices filtrados porque `sqlcmd` tenía `QUOTED_IDENTIFIER OFF`. Se agregó `SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON;` al script y se re-ejecutó con éxito (idempotente). |
| `sql/migraciones_saas_02_procedimientos.sql` | Exitosa. |
| `sql/migraciones_saas_03_archivos.sql` | Exitosa; `LogoRuta` quedó en `/Attachment/1/institucion/...`. |

**Movimiento físico:** `Attachment/institucion` → `Attachment/1/institucion`; `Attachment/3627` → `Attachment/1/3627`.

**Identidades:** se reseedearon las tablas vacías para dejar el clon limpio. `DatosInstitucion` próximo Id = 2; `Paciente`/`TipoConsulta` próximo Id = 1.

---

## Verificación aplicada

- 11 columnas `ConsultorioID` **NOT NULL** con FK a `DatosInstitucion`.
- `Usuario.Password` → `nvarchar(250)` (`max_length=500`).
- `Usuario.CompaniaID` **NOT NULL**; `admin` e `ivanbartolo` en consultorio 1.
- FKs `CitaPaquete`: `CitaID → Cita.Id` y `PacientePaqueteID → PacientePaquete.Id`.
- 6 índices únicos (`Usuario.UserName`, `DatosFiscales.RFC/RazonSocial/DatosInstitucionID`, `DatosInstitucion.Nombre/Email`).
- `DatosInstitucion.LogoRuta` actualizado y archivo existente en `Attachment/1/institucion/...`.
- Firmas de SPs:
  - `sp_nh_AutenticacionUsuario` solo `@UserName`.
  - `sp_nh_SaveUsuario` con `@Password nvarchar(250)`.
  - `GetAllTipoConsulta` con `@SoloActivos`.
  - `sp_nh_SaveCita`/`Save*` con `@ConsultorioID`.

## Smoke test transaccional (rollback)

- Alta de tenant nuevo + fiscal + usuario + paciente dentro de una transacción.
- `Paciente` tenant 1 = 0; tenant nuevo = 1.
- `GetAllPacienteByEstatus` tenant 1 vacío; tenant nuevo con 1 fila.
- `sp_nh_GetPacienteById` con tenant ajeno devuelve 0 filas.
- `sp_nh_SavePaciente` con tenant ajeno devuelve `NULL`.
- Rollback: 0 filas de prueba persistidas.

## Estado final de datos

- `DatosInstitucion`: 1 fila (Dra. Tania, Id=1).
- `DatosFiscales`: 0 filas (consultorio 1 vacío, correcto).
- `Usuario`: 2 filas (`admin`, `ivanbartolo`; ambos consultorio 1).
- 11 tablas de dominio: 0 filas.
- `admin`/`ivanbartolo` conservan password legado en texto plano; se rehashean a PBKDF2 en el primer login exitoso.

## Pendientes

- Matriz manual/UI M1–M19: login/rehash, registro desde UI, branding, anexos, receta, CRUD TipoConsulta, etc.
- No se tomó respaldo previo (0.4). El clon tenía 0 filas de dominio y las migraciones fueron aditivas; considerar un respaldo post-migración antes de las pruebas de UI.
- Envío de correo de bienvenida/credenciales: pendiente en Engram (`project/consultoriomedico-saas-email-pending`).
- No commitear `sql/reset_datos_consultorio.sql`.
