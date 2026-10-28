# Design: Ajustes al Expediente Clínico

## Contexto

`Expediente.cshtml` (change `expediente-clinico-historial` + `expediente-acciones`) muestra en Tab 1 `#tratamientosTable` alimentado por `sp_nh_GetTratamientosPorPaciente`, que hoy hace `UNION` de Cita + Consulta. La WinForms original (§2 de `docs/evidencia-historial-clinico-winforms-descompilado.md`) muestra en "Historial de Tratamientos" (`dgvCita`) **SOLO citas**. Este change corrige el SP a solo-citas, formatea la fecha del grid y ajusta los dos modales autocontenidos. Cambio quirúrgico: **solo SQL + vista**; DbWrapper/DTO/endpoints/controllers/services intactos.

## Decisiones de Arquitectura

| # | Decisión | Rationale | Alternativa rechazada |
|---|----------|-----------|----------------------|
| D1 | SP devuelve **solo Cita** (quitar rama `UNION ... Consulta`); conservar `'Cita' AS Origen` constante y `ORDER BY c.FechaCita DESC` | Fiel a la WinForms (§2: historial de tratamientos = `dgvCita` → `GetAllCitaByPacienteID`); `Origen` constante preserva el contrato `TratamientoHistoricoDTO` y el endpoint sin tocar DTO/mapeo | Quitar también `Origen` (amplía superficie a DTO+mapeo+columna sin ganancia funcional) |
| D2 | Actualizar SP **existente** con patrón idempotente DROP+CREATE (`IF OBJECT_ID(...) IS NOT NULL DROP PROCEDURE ...; GO; CREATE PROCEDURE ...`) | El script actual usa `IF OBJECT_ID IS NULL` (solo crea si no existe) → nunca re-aplica el fix. DROP+CREATE es seguro: objeto `sp_nh_*` propio de la migración, SELECT-only (sin datos), única dependencia `DbWrapper.Historial.cs` | `ALTER PROCEDURE` (no portable ante cambios de firma/body) / mantener `IF ... IS NULL` (el bug persiste en BD) |
| D3 | **Reescribir** `sql/sp_nh_Historial.sql` en su lugar (no `_v2`) | Convención del repo: un archivo por módulo (`sp_nh_Cita.sql`, `sp_nh_Consulta.sql`). Un `_v2` dejaría dos definiciones competidoras del mismo SP; Git conserva la versión UNION previa como rollback | Crear `sql/sp_nh_Historial_v2.sql` (rompe convención + duplica definición) |
| D4 | Formato fecha con JS manual `formatFechaHoraAmPm(iso)` en el `render` de la columna `Fecha` (sin moment.js) | El stack no carga moment.js (solo Bootstrap 5 + DataTables 1.13.6). ISO de `JsonConvert` (`2026-09-05T18:00:00`, sin offset) parsea como hora **local**, igual al wall-clock guardado en BD | moment.js (dependencia nueva) / plugin `datetime` de DataTables (dependencia extra) |
| D5 | Modales: Comentarios→`<textarea rows="4">` en ambos; quitar `Descripcion` y `DescripcionReceta` del modal Consulta del expediente | `sp_nh_SaveConsulta` declara `@Descripcion NVARCHAR(500)=NULL` y `@DescripcionReceta NVARCHAR(1000)=NULL`; al no enviarse quedan `null` en `Consulta` → `(object)x ?? DBNull.Value` → NULL en BD. No rompe el guardado | Añadir parámetros nuevos / marcar required (viola spec "sin cambios de contrato") |

## Diagramas de Secuencia

Solo cambia el paso final de obtención de datos (Flujo 2 del design previo se mantiene intacto):

```
Expediente.cshtml (DataTable init) ──> GET Historial/ConsultarTratamientosPorPaciente [MVC]
  └─> HistorialService (MVC) ──> api/Historial/TratamientosPorPaciente/{id} [WebApi]
        └─> DbWrapper.ObtenerTratamientosPorPaciente
              └─> sp_nh_GetTratamientosPorPaciente   (SOLO Cita, ya sin UNION)
  <── JSON ModelResponse<List<TratamientoHistoricoDTO>>   → render Fecha via formatFechaHoraAmPm
```

## Detalle por Capa (solo SQL + Vista)

### SQL — `sql/sp_nh_Historial.sql` (reescribir, BOM UTF-8)

Reemplazar el bloque `IF OBJECT_ID(...) IS NULL ... EXEC('CREATE ...')` por DROP+CREATE. El `GO` separa lotes, así `CREATE` ya no necesita el wrapper `EXEC('...')`:

```sql
IF OBJECT_ID('dbo.sp_nh_GetTratamientosPorPaciente', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_nh_GetTratamientosPorPaciente;
GO

CREATE PROCEDURE [dbo].[sp_nh_GetTratamientosPorPaciente]
(
    @IdPaciente INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.Id, c.FechaCita AS Fecha, t.Nombre AS TratamientoNombre,
           'Cita' AS Origen, c.Comentarios
    FROM dbo.Cita c
        LEFT JOIN dbo.Tratamiento t ON c.TratamientoID = t.Id
    WHERE c.PacienteID = @IdPaciente AND c.Estatus = 1
    ORDER BY c.FechaCita DESC;
END
GO
```

Columnas del result set sin cambio (`Id int, Fecha datetime, TratamientoNombre nvarchar, Origen nvarchar, Comentarios nvarchar`), por lo que el mapper de `DbWrapper.Historial.cs` sigue válido.

### Vista — `Views/Historial/Expediente.cshtml` (modificar)

**1) Render de fecha** — añadir la función global y usarla en la columna `Fecha` de `#tratamientosTable`:

```js
function pad2(n) { return n < 10 ? '0' + n : n; }

function formatFechaHoraAmPm(iso) {
    if (!iso) return '';
    var d;
    if (typeof iso === 'string' && iso.indexOf('/Date(') === 0) {
        d = new Date(parseInt(iso.substr(6), 10));      // defensivo: /Date(ms)/
    } else {
        d = new Date(iso);                              // ISO sin offset → hora local
    }
    if (isNaN(d.getTime())) return '';                  // fecha inválida → celda vacía
    var hh = d.getHours();
    var ampm = hh >= 12 ? 'pm' : 'am';
    var h12 = hh % 12; if (h12 === 0) h12 = 12;
    return pad2(d.getDate()) + '/' + pad2(d.getMonth() + 1) + '/' + d.getFullYear()
         + ' ' + pad2(h12) + ':' + pad2(d.getMinutes()) + ' ' + ampm;
}
```

Columna (cambia `{ data: 'Fecha' }` por): `{ data: 'Fecha', render: function (data) { return formatFechaHoraAmPm(data); } }`.

Manejo de null/''/inválida: devuelve `''` (celda vacía), cumple spec "Fecha nula → vacío". ISO sin offset (`2026-09-05T18:00:00`) lo parsea `new Date` como hora **local**, manteniendo el wall-clock de BD.

**2) Modal "Nuevo Tratamiento"** — sustituir el `<input type="text" ... name="Comentarios" ...>` por:

```html
<textarea class="form-control" name="Comentarios" rows="4" placeholder="Comentarios de la cita"></textarea>
```

`serialize()` serializa `<textarea>` igual que `<input>` (por `name`); `Comentarios` viaja idéntico por `$('#citaFormExpediente').serialize()`.

**3) Modal "Nueva Consulta"** — eliminar los bloques `Descripción` (`<input type="text" name="Descripcion">`) y `Receta` (`<textarea name="DescripcionReceta" rows="2">`); cambiar el `Comentarios` existente de `rows="2"` a `rows="4"`. Al no existir los campos, el model-binding deja `Consulta.Descripcion` y `Consulta.DescripcionReceta` en `null`; `DbWrapper.Consulta.cs` ya mapea `(object)c.Descripcion ?? DBNull.Value` → NULL, y `sp_nh_SaveConsulta` los acepta con default `NULL`. El POST a `Consulta/GuardarOActualizarConsulta` sigue funcionando. `Views/Consulta/Index.cshtml` NO se toca (conserva Descripción/Receta).

## Verificación

- **Build**: MSBuild Debug de `DESI.ConsultorioMedico.sln` → exit 0; `aspnet_compiler` precompilando vistas (valida Razor C# 5 de `Expediente.cshtml`) → exit 0.
- **SP en BD productiva** (`DB_9C7990_Consultorio`, solo con aprobación): ejecutar el script (DROP+CREATE).
  - `EXEC sp_nh_GetTratamientosPorPaciente @IdPaciente = 19803;` → **0 filas** (paciente con 1 consulta y 0 citas).
  - `EXEC sp_nh_GetTratamientosPorPaciente @IdPaciente = 3627;` → **2 filas**, todas `Origen = 'Cita'`.
- **Navegador**: fecha `dd/MM/yyyy hh:mm am/pm`; Comentarios textareas amplias; modal Nueva Consulta sin Descripción/Receta; `Consulta/Index.cshtml` intacto.

## Riesgos

- DROP del SP en BD productiva: objeto propio SELECT-only, sin datos; ejecutar solo con aprobación (mitigación D2/D3).
- Formato de fecha dependa del JSON recibido: render defensivo (ISO o `/Date()`); verificar en navegador (D4).

## Open Questions

Ninguna que bloquee. Nota heredada: tratamientos de paquetes quedan fuera (change `paquetes-del-paciente`).
