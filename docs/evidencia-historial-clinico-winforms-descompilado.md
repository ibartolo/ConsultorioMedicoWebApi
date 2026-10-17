# Evidencia — Historial Clínico en la WinForms original (descompilación)

> Generado el 2026-09-04. Base para el diseño de los changes `expediente-acciones` y
> `paquetes-del-paciente` de la migración MVC + WebApi.
> Método: descompilación con `ilspycmd` del build ClickOnce instalado en
> `C:\Users\Wayssen\AppData\Local\Apps\2.0\1RQBY7JA.QTM\ZRE37GTL.QTE\cons..tion_0000000000000000_0001.0000_92357ddea951e264\`
> (DLLs: `ConsultorioMedico.exe`, `Desi.ConsultorioMedico.Data.{BL,DA,VO}.dll`).

---

## 1. Formulario objetivo: `CatalogoHistorial`

- Namespace `ConsultorioMedico.Catalogos`, clase `CatalogoHistorial`.
- Usa `DevComponents.DotNetBar.TabControl` (NO TabControl de WinForms) con Tabs:
  - `tabItem1` = **"Información Paciente"** (visible por defecto)
  - `tabItem3` = **"Historial de consultas"**
  - `tabItem2` = **"Archivos"**
  - `tabItem8` = **"Paquetes"**
  - `tabItem4` = **"Ultima Consulta"** → `Visible=false` (oculto en la app)
- Panel lateral "Acciones" con botones:
  - `btnGuardar` = "Guardar"
  - `btnNuevaCita` = **"Nuevo Tratamiento"** ← el botón está ETIQUETADO "Nuevo Tratamiento"
  - `btnNuevaConsulta` = "Nueva Consulta"
  - `btnEliminar` = "Eliminar Historial Clinico"
  - `btnCerrar`

## 2. Grids por pestaña (respuesta a: "¿qué muestran los DataGridView?")

| Grid | Pestaña | Carga | Origen de datos | Nota |
|---|---|---|---|---|
| `dgvCita` | **Información Paciente** → `groupPanel3` "Historial de Tratamientos" | `CitaBL.GetAllCitaByPacienteIdDT(paciente.Id)` | **SP `GetAllCitaByPacienteID`** | Columnas visibles: Tratamiento (`Tratamiento.Nombre`), FechaCita → "Fecha de cita", Estado (LED verde/rojo). Oculta Id/Estatus/PacienteId/TratamientoId/Estado. Doble clic → `DetalleCita`. |
| `dgvConsultas` | **Historial de consultas** | `ConsultaBL.GetAllConsultaByPacienteIdDT(paciente.Id)` | **SP `GetAllConsultaByPacienteID`** | Columnas: FechaConsulta → "Fecha de Consulta", Nombre → "Consulta" (TipoConsulta), DescripcionReceta → "Receta". Filtro por Descripcion. |
| `dgvDocumento` | **Archivos** | `AnexoBL.GetAllAnexoByPacienteIdDT` | (Anexo) | Oculta Id/Estatus/Ruta. |
| `dgvPaquetes` | **Paquetes** → `groupPanel6` "Historial de Paquetes Paciente" | `PacientePaqueteBL.GetAllPacientePaqueteByPacienteIdDT` | **SQL inline** (join Paquete + PacientePaquete) | Devuelve Id, Nombre, PacienteID, PacientePaqueteID. |
| `dgvCitasInPaquete` | **Paquetes** → `groupPanel7` "Historial de Tratamientos de Paquete" | `CitaPaqueteBL.GetAllCitaPaqueteBypacientePaqueteIdDT(pacientepaquete.Id)` | **SQL inline** | Devuelve Cita + Tratamiento.Nombre. |
| Pagos | **Paquetes** → etiquetas `lbPrecio/lbpagado/lbAdeuda` | `PagoPaqueteBL.GetAllPagoPaqueteByPacientePaqueteIdDT` | **SP `GetAllPagoPaqueteByPacientePaqueteIdDT`** | Precio / Pagado / Adeuda. |

**Conclusión clave**: **NO existe** un grid "TratamientosHistoricos" dedicado. El historial de
tratamientos del paciente ES el grid de **Citas** (`dgvCita`): cada cita lleva un
`TratamientoID` opcional y se muestra el nombre del tratamiento. Tampoco existe entidad
"paciente-recibió-tratamiento" en la capa VO.

## 3. Botones "Nuevo Tratamiento" y "Nueva Consulta"

Ambos usan el campo **estático** `public static Paciente _paciente` como puente de contexto:

```csharp
// btnNuevaCita_Click (etiquetado "Nuevo Tratamiento"):
_paciente = paciente;
AltaCita altaCita = new AltaCita();
altaCita.ShowDialog();
_paciente = null;
RecargarGridCitas();

// btnNuevaConsulta_Click:
_paciente = paciente;
NuevaConsulta nuevaConsulta = new NuevaConsulta();
nuevaConsulta.ShowDialog();
_paciente = null;
CargarGridConsultas();
```

- `AltaCita.Load`: si `CatalogoHistorial._paciente != null` → selecciona al paciente en el combo.
- `NuevaConsulta.Load`: igual (primera rama edita consulta sembrada desde `BandejaHorariosST`).
- **Conclusión**: NO hay botón que escriba un Tratamiento suelto. "Nuevo Tratamiento" = crear una
  **Cita** (fecha+hora+tratamiento+comentario); el tratamiento queda asociado vía `Cita.TratamientoID`.

## 4. Guardado: NHibernate, NO SPs (respuesta a "¿NHibernate o procedures?")

- Todas las escrituras van por **NHibernate genérico** `BaseDataAccess.Save(object)` →
  `session.SaveOrUpdate` + `transaction.Commit()` (Fluent NHibernate, MsSql2008).
- `CitaBL.SaveCita` (NO existe `CitaDA.SaveCita`), `ConsultaBL.SaveConsulta`,
  `PacientePaqueteBL.SavePacientePaquete`, `PagoPaqueteBL.SavePagoPaquete`,
  `TratamientoPaqueteBL.SaveTratamientoPaquete`.
- El botón "Guardar Tratamiento" de la pestaña Paquetes (`bntGuardarTratamiento_Click`) crea
  **Cita + CitaPaquete** juntos (`new BaseDataAccess().Save(list)`), nunca un Tratamiento suelto.
- Las **lecturas de grid** sí usan SPs/SQL (ver tabla §2).
- Por la regla del proyecto (**NHibernate muere → todo por SPs `sp_nh_*`**), en la web estos
  guardados se implementan con SPs nuevos: `sp_nh_SaveCita` (ya existe), `sp_nh_SaveConsulta`
  (ya existe), y para paquetes los que se creen en el change `paquetes-del-paciente`.

## 5. Validación de disponibilidad de calendario: NO EXISTE

- Los SPs `GetCitaByFechaCita` y `GetConsultaByFechaConsulta` existen en BL/DA pero
  **ningún formulario los invoca** (cero call sites en los forms descompilados).
- Al guardar `NuevaConsulta`/`AltaCita` **no se valida choque de horarios**.
- La sensación de "validar calendario" venía de la vista `BandejaHorarios` (DevComponents
  Schedule) que muestra citas/consultas por período — visual, no validación al guardar.

## 6. Entidades VO relevantes

- `Cita` { Id, Estatus, FechaCita, Paciente→PacienteID, Tratamiento→TratamientoID (nullable), Comentarios, Estado }
- `Consulta` { Id, Estatus, FechaConsulta, PacienteID, TratamientoID (nullable), CitaID (nullable),
  TipoConsultaID (nullable), Descripcion, DescripcionReceta, Comentarios, FechaProximaConsulta }
- `Tratamiento` = catálogo puro { Id, Nombre, Duracion, Descripcion, Estatus } — **sin referencia a Paciente**
- Asociaciones: `PacientePaquete` {Paciente,Paquete}, `PagoPaquete` {Pago,FechaPago,PacientePaquete},
  `CitaPaquete` {Cita,PacientePaquete}, `TratamientoPaquete` {Paquete,Tratamiento}

## 7. Implicaciones para la migración web

1. **Tab 1 "tratamientos del paciente"** ya implementada en `expediente-clinico-historial`
   (grid derivado Cita+Consulta vía `sp_nh_GetTratamientosPorPaciente`) es **fiel** a la original.
   No crear tabla "Paciente-Tratamiento".
2. **Botón "Nuevo Tratamiento" en la web = modal de Cita** (paciente fijo) — reutilizar el modal
   `#citaForm` de `Cita/Index.cshtml` (POST `api/Cita/Guardar` → `sp_nh_SaveCita`, ya existe).
3. **Botón "Nueva Consulta" = modal de Consulta** (paciente fijo) — reutilizar `#consultaForm`
   de `Consulta/Index.cshtml` (POST `api/Consulta/Guardar` → `sp_nh_SaveConsulta`, ya existe).
4. **No implementar validación de choque de horarios** (la original no la tenía). Si el usuario
   la pidiera después, sería una mejora nueva, no una réplica.
5. **Pestaña Paquetes** (change futuro `paquetes-del-paciente`): replicar layout 2 secciones:
   izquierda `dgvPaquetes` (paquetes adquiridos) + derecha detalle (tratamientos del paquete,
   comentarios, nuevo pago, Precio/Pagado/Adeuda, historial de citas-tratamientos del paquete).
   Las escrituras de la WinForms (NHibernate) se convierten en SPs `sp_nh_*`.
