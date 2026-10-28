# Tasks: Ajustes al Expediente Clínico

## Fase 1: SQL — SP solo-Citas

- [x] 1.1 Reescribir `sql/sp_nh_Historial.sql`: reemplazar el bloque `IF OBJECT_ID(...) IS NULL ... EXEC('CREATE ...')` por el patrón idempotente `IF OBJECT_ID('dbo.sp_nh_GetTratamientosPorPaciente','P') IS NOT NULL DROP PROCEDURE dbo.sp_nh_GetTratamientosPorPaciente; GO` + `CREATE PROCEDURE ...`. Cuerpo con ÚNICA rama Cita: `SELECT c.Id, c.FechaCita AS Fecha, t.Nombre AS TratamientoNombre, 'Cita' AS Origen, c.Comentarios FROM dbo.Cita c LEFT JOIN dbo.Tratamiento t ON c.TratamientoID = t.Id WHERE c.PacienteID = @IdPaciente AND c.Estatus = 1 ORDER BY c.FechaCita DESC;`. Verificación: sin `UNION`.
- [x] 1.2 Confirmar que el script no conserve referencia a `Consulta` ni `UNION`. Verificación: búsqueda de `UNION` y `Consulta` en el body del SP → sin coincidencias.

## Fase 2: Vista — formato de fecha

- [x] 2.1 En `ConsultorioMedicoMVC/Views/Historial/Expediente.cshtml`, dentro de `@section scripts`, agregar `function pad2(n)` y `function formatFechaHoraAmPm(iso)` (JS manual defensivo: `null`/`''` → `''`; `/Date(ms)` → `new Date(parseInt(iso.substr(6),10))`; ISO → `new Date(iso)`; inválida → `''`; salida `dd/MM/yyyy hh:mm am/pm`). Verificación: función presente, sin moment.js.
- [x] 2.2 Cambiar la columna `Fecha` de `#tratamientosTable` de `{ data: 'Fecha' }` a `{ data: 'Fecha', render: function (data) { return formatFechaHoraAmPm(data); } }`. Verificación: `render` aplicado; formato `dd/MM/yyyy hh:mm am/pm`.

## Fase 3: Vista — modal Nuevo Tratamiento

- [x] 3.1 En `Expediente.cshtml`, modal `#modalCitaExpediente`, sustituir `<input type="text" class="form-control" name="Comentarios" placeholder="Comentarios de la cita">` por `<textarea class="form-control" name="Comentarios" rows="4" placeholder="Comentarios de la cita"></textarea>`. Verificación: `$('#citaFormExpediente').serialize()` sigue enviando `Comentarios` por `name`.

## Fase 4: Vista — modal Nueva Consulta

- [x] 4.1 En `Expediente.cshtml`, modal `#modalConsultaExpediente`, eliminar el bloque `Descripción` (`<input type="text" class="form-control" name="Descripcion" ...>`) y el bloque `Receta` (`<textarea class="form-control" name="DescripcionReceta" rows="2" ...>`). Verificación: no existen `name="Descripcion"` ni `name="DescripcionReceta"` en el modal.
- [x] 4.2 Cambiar el `Comentarios` del modal Consulta de `rows="2"` a `rows="4"`. Verificación: `<textarea class="form-control" name="Comentarios" rows="4" ...>`.
- [x] 4.3 Verificar aislamiento del change: `ConsultorioMedicoMVC/Views/Consulta/Index.cshtml`, `Views/Cita/Index.cshtml` e `Historial/Index` intactos. Verificación: `Consulta/Index.cshtml` conserva Descripción/Receta.

## Fase 5: Verificación

- [ ] 5.1 Build: `MSBuild DESI.ConsultorioMedico.sln /t:Build /p:Configuration=Debug` → exit 0. (BLOQUEADO por defecto de entorno pre-existente: falta `NuGet.targets` en `MSBuild\Current\Bin\amd64` → PackageReference no resuelve; no relacionado con este change que solo toca SQL + .cshtml. `nuget /t:Restore` OK y `aspnet_compiler` OK.)
- [x] 5.2 `aspnet_compiler` precompilando vistas → exit 0 (valida Razor C# 5 de `Expediente.cshtml`).
- [x] 5.3 Ejecutar el SP actualizado en BD productiva (DROP+CREATE, solo con aprobación) y validar `EXEC sp_nh_GetTratamientosPorPaciente @IdPaciente=19803` → 0 filas; `@IdPaciente=3627` → 2 filas (todas `Origen='Cita'`).
- [x] 5.4 Verificar BOM UTF-8 en archivos tocados (`sql/sp_nh_Historial.sql`, `Expediente.cshtml`). Verificación: sin caracteres mojibake.
- [ ] 5.5 Navegador: fecha `dd/MM/yyyy hh:mm am/pm`; Comentarios textareas amplias en ambos modales; modal Nueva Consulta sin Descripción/Receta; `Consulta/Index.cshtml` intacto. (No ejecutado en entorno headless; cambios verificados estáticamente por grep + aspnet_compiler.)
