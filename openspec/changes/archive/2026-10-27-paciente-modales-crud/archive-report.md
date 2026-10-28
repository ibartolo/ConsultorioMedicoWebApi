# Archive Report — paciente-modales-crud

**Change**: paciente-modales-crud
**Fecha de archive**: 2026-10-27
**Estado**: PASS (implementado y verificado)
**Modo**: openspec (file-based)

---

## Resumen

Cambio archivado tras implementación y verificación exitosa. Migra el alta/edición de Paciente de página full-page (`Nuevo.cshtml`) a un modal `modal-xl` reutilizable (`_Formulario.cshtml`), añade baja lógica (Eliminar → `Estatus=0`) hoy inexistente, y refresca SOLO la fila afectada en DataTables (sin `ajax.reload()` ni `window.location`). Confirmaciones con SweetAlert2. Verificación: MSBuild exit 0 + `aspnet_compiler` exit 0 + SP `sp_nh_DeletePaciente` confirmado en BD productiva.

## Sincronización de specs (merge a main spec)

Delta `specs/paciente/spec.md` → `openspec/specs/paciente/spec.md` (capability `paciente` preexistente, archivada en `2026-08-24-dominio-paciente`).

| # | Requisito (ADDED) | Acción |
|---|-------------------|--------|
| 1 | Alta y edición en modal | Agregado |
| 2 | Carga de datos para edición | Agregado |
| 3 | Guardado con validación y confirmación | Agregado |
| 4 | Refresco de fila afectada | Agregado |
| 5 | Eliminación lógica de Paciente | Agregado |
| 6 | Backend de eliminación | Agregado |
| 7 | No regresión | Agregado |

**Requisitos previos preservados**: Entidad Paciente, ValueObjects Email/Telefono/Sexo, Sobre público `ModelResponse<T>`, DTOs request/response, `GET /api/v1/paciente`, `GET /api/v1/paciente/{id}`, `POST /api/v1/paciente/guardar`, Flujo de datos por endpoint. Ninguno fue borrado ni duplicado.

## Reconciliaciones aplicadas

1. **`GuardarOActualizarPaciente` devuelve `ModelResponse<Paciente>` (con `Id`), no `ModelResponse<int>`**.
   - Main spec `POST /api/v1/paciente/guardar`: `ModelResponse<int>` → `ModelResponse<Paciente>` (entidad con `Id` ya seteado).
   - Scenarios "Crear paciente" y "Actualizar paciente": `Response` es la entidad `Paciente` (no un `int`).
   - Main spec "Flujo de datos por endpoint" / "Flujo de guardado": `ModelResponse<int>` → `ModelResponse<Paciente>`.
2. **Eliminación lógica `Estatus=0`**: cubierta por los requisitos ADDED "Eliminación lógica de Paciente" y "Backend de eliminación" (SP `sp_nh_DeletePaciente` → `UPDATE dbo.Paciente SET Estatus=0 WHERE Id=@Id`). Sin conflicto con specs previas.
3. **Ids inválidos → HTTP 200 + `IsSuccess=false`**: el requisito "Backend de eliminación" especifica `id<=0` → `IsSuccess=false` (y `filas<=0` → "Paciente no encontrado."), consistente con la convención del proyecto ("HTTP 200 siempre", `ModelResponse<T>`).

## Contenido del archive

- `proposal.md` ✅
- `specs/paciente/spec.md` ✅ (delta: 7 ADDED requirements, 13 scenarios)
- `design.md` ✅ (D1–D8)
- `tasks.md` ✅ (24/25 completadas; tarea 6.3 prueba manual pendiente como SUGGESTION no bloqueante)
- `verify-report.md` ✅ (Verdict: PASS — 0 CRITICAL, 0 WARNING, 2 SUGGESTION)

## Verificación del archive

- [x] Main spec actualizado correctamente (`openspec/specs/paciente/spec.md`, 285 líneas).
- [x] Carpeta del change movida a `openspec/changes/archive/2026-10-27-paciente-modales-crud/`.
- [x] Archive contiene todos los artefactos (proposal, specs, design, tasks, verify-report).
- [x] `openspec/changes/` ya no contiene `paciente-modales-crud`.

## Notas

- **`paquetes-del-paciente` NO fue tocado** — permanece activo como propuesta futura full-scope (siguiente en secuencia si aplica).
- **SUGGESTION-1**: el servicio MVC compone `NombreCompleto` con `string.Join(" ", ...).Where(!IsNullOrWhiteSpace)` (más robusto que el `$""` del design D3) — desviación positiva documentada en verify-report.
- **SUGGESTION-2**: tarea 6.3 (prueba manual en navegador) quedó pendiente por entorno headless; recomendada una pasada manual post-archive.
- El SP `sp_nh_DeletePaciente` ya existe en BD productiva (Smarterasp), definición idéntica al archivo `sql/sp_nh_Paciente.sql` (confirmado en verify-report).
