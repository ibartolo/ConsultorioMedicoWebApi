# Propuesta: Registro SaaS de Consultorio + Aislamiento Multi-Tenant

## Intent

Primera entrega SaaS del Consultorio Médico: permitir el registro público de un consultorio (creando su tenant `DatosInstitucion` + primer usuario admin) y aislar los datos por consultorio para que cada tenant vea y gestione solo lo suyo. Se elimina el modelo de "institución/datos-fiscales únicos globales" y se adopta filtrado por `ConsultorioID` derivado del usuario autenticado (nunca del cliente).

## Scope

### In Scope
- Registro público (ventana de captura) que crea consultorio + fila fiscal 1:1 + usuario admin (`UserName` = RFC, único global).
- Autenticación por `UserName` con PBKDF2 (portar `Cryptography` de HelpDesk), rehash transparente de passwords legados, `Usuario.Password` nvarchar(50)→nvarchar(250), validar `Estatus = 1`.
- Aislamiento: agregar `ConsultorioID` a Paciente, Cita, Consulta, Anexo, PacientePaquete, PagoPaquete, CitaPaquete, Tratamiento, TratamientoPaquete, Paquete, TipoConsulta; filtrar y validar pertenencia en todos los endpoints/pantallas (Dashboard, Historial, Paciente, Cita, Consulta, Anexo, Usuario, DatosInstitucion/DatosFiscales).
- CRUD de `TipoConsulta` por consultorio (hoy solo GET List).
- Branding por tenant (sidebar + receta) desde `DatosInstitucion`; login sin logo/nombre.
- Migraciones (ALTER TABLE) en scripts nuevos aprobados: columnas `ConsultorioID` + backfill a consultorio 1, corregir FKs de `CitaPaquete`, reubicar logo/anexos bajo `Attachment/{consultorioId}/…`.

### Out of Scope
- Roles/permisos, trial, vigencia, licencia, pagos, límites (solo 2 consultorios piloto).
- App superadmin/proveedor (pasante separado).
- Ramas/BD `-dermatologia` (no se tocan). No existe BD productiva SaaS aún.
- Seeds de catálogos (un consultorio nuevo inicia con catálogos vacíos).

## Capabilities

### New Capabilities
- `registro-saas`: registro público del consultorio + primer admin (unicidad RFC/email/NombreComercial/RazonSocial/UserName; fiscal 1:1).
- `aislamiento-tenant`: resolución y filtrado por consultorio en toda la aplicación.
- `autenticacion-saas`: login por `UserName` + PBKDF2 + rehash legado.
- `tipo-consulta-crud`: CRUD de `TipoConsulta` por consultorio.

### Modified Capabilities
- `paciente`: `ConsultorioID` + filtro por tenant + anexos bajo `Attachment/{consultorioId}/{pacienteId}`.
- `cita`: `ConsultorioID` + filtro por tenant.
- `expediente-clinico`: filtro por tenant.
- `paquetes-lectura`: filtro por tenant.

## Approach

Mantener el flujo existente (View → MVC → WebApi → DbWrapper → SP). El tenant se obtiene del usuario autenticado: WebApi desde el claim `empresaId` (`BaseController.ObtenerEmpresaIdDesdeClaim`) y MVC desde `TokenCookie.EmpresaID`; se inyecta como parámetro en cada SP (prefijo `sp_nh_`) y se valida pertenencia antes de leer/guardar. El registro usa los SPs de alta de `DatosInstitucion`/`DatosFiscales`/`Usuario` extendidos para unicidad y 1:1 fiscal. Migraciones aditivas en `sql/*.sql` con backfill a consultorio 1 ("Dra. Tania Guadalupe Rios Bartolo"); `ivanbartolo` (CompaniaID NULL) pasa al consultorio 1; `admin` se conserva; `DatosFiscales` del consultorio 1 queda vacío.

## Affected Areas

| Area | Impact | Descripción |
|------|--------|-------------|
| `sql/migraciones_saas_*.sql` | New | ALTER TABLE: +`ConsultorioID` (11 tablas) + corregir FKs `CitaPaquete` + `Usuario.Password`; backfill a 1 |
| `ConsultorioMedicoEntities/Catalogos/*` | Modified | +`ConsultorioID` en entidades/DTOs; entidades/DTO/validador de `TipoConsulta` |
| `ConsultorioMedicoWebApi/DAL/DbWrapper.*`, `Services/*`, `Controllers/*` | Modified | filtro por tenant + validación pertenencia + SPs de registro/auth |
| `ConsultorioMedicoWebApi/Controllers/AutenticacionController.cs` + `Helpers/Cryptography.cs` | New/Modified | PBKDF2 + login por `UserName` + rehash |
| `ConsultorioMedicoMVC/*` (controllers, services, HttpClientConnection, Views) | Modified | `TokenCookie.EmpresaID`, pantallas por tenant, ventana de registro público |
| `ConsultorioMedicoWebApi/Attachment/` | Modified | logo/anexos reubicados bajo `{consultorioId}` |

## Risks

| Riesgo | Prob. | Mitigación |
|--------|-------|-----------|
| Backfill masivo a consultorio 1 corrompe datos | Med | Migración idempotente con `WHERE ConsultorioID IS NULL`; respaldo previo; solo clon develop/main |
| Rehash legado falla en primer login | Med | Verificación PBKDF2 con fallback a texto plano → rehash; pruebas con `admin`/`ivanbartolo` |
| FKs incorrectas de `CitaPaquete` | High | Corregir en migración antes de habilitar escritura |
| Fuga de tenant (endpoint sin filtro) | High | `BaseController` fuerza tenant del claim; revisión por módulo; nunca aceptar tenant del cliente |

## Open Questions

- Ninguna bloqueante para specs. Decisiones confirmadas: dirección con columnas existentes (`Calle`, `NoExterior`, `NoInterior`, `Colonia`, `Municipio`, `Estado`, `CP`, `Pais`), `Ciudad`→`Municipio`; RFC con validación de formato de HelpDesk + unicidad; el orden de liberación de los 2 pilotos queda para operación.

## Acceptance Criteria

- [ ] Registro público crea consultorio + fila fiscal 1:1 + admin (`UserName`=RFC) con unicidad global.
- [ ] Login por `UserName` con PBKDF2; rehash transparente; `Estatus=1`.
- [ ] Cada endpoint/pantalla filtra por tenant y rechaza datos ajenos con error controlado.
- [ ] Consultorio 1 backfilleado; `admin`/`ivanbartolo` operan sobre consultorio 1; `DatosFiscales` del consultorio 1 vacío.
- [ ] CRUD de `TipoConsulta` por consultorio funcional.
- [ ] Branding (sidebar + receta) por tenant; login sin branding.
- [ ] MSBuild Debug + `aspnet_compiler` exit 0.

## Migration Impact

- ALTER TABLE en el clon `db_9c7990_consultoriodesitst` (solo develop/main): +`ConsultorioID` (NOT NULL con backfill a 1) en las 11 tablas de dominio; corregir `FK_CitaPaquete_Cita` y `FK_CitaPaquete_PacientePaquete`; `Usuario.Password` → nvarchar(250); reubicar `Attachment/institucion/...` → consultorio 1 y actualizar `DatosInstitucion.LogoRuta`/`Anexo.Ruta`. Scripts nuevos `sql/migraciones_saas_*.sql` con `ALTER TABLE` idempotente; no destructivos para SPs existentes. Aprobación explícita del usuario antes de ejecutar.

## Rollback Plan

Todo aditivo y versionable: revertir con `git checkout` de archivos nuevos/modificados. Las migraciones se entregan como scripts separados que solo se ejecutan en el clon develop/main previa aprobación; para revertir datos, restaurar respaldo del clon previo a la migración. El login conserva fallback a texto plano hasta confirmar el rehash, permitiendo rollback de autenticación sin bloqueo.
