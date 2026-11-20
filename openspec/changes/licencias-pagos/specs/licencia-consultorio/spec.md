# licencia-consultorio Specification

## Purpose

Modelo de licencia anual del consultorio sobre `DatosInstitucion` (sin tabla de licencia aparte), pantalla `Licencia` accesible a todos los usuarios, y datos bancarios/monto desde configuración. Plan Básico $1,200.00 MXN anual; Intermedio/Avanzado solo modelados "en desarrollo".

## Requirements

### Requirement: Modelo de licencia en DatosInstitucion

`DatosInstitucion` MUST incorporar: `Estatus BIT`, `FechaVigenciaInicio DATETIME NULL`, `FechaVigenciaFin DATETIME NULL`, `EsPeriodoPrueba BIT`, `Plan NVARCHAR(20)` con valores `Basico|Intermedio|Avanzado` y default `Basico`. El sistema MUST NOT crear una tabla de licencia separada.

#### Scenario: Columnas presentes

- GIVEN la migración aplicada
- WHEN se inspecciona `DatosInstitucion`
- THEN existen las columnas de licencia con `Plan` default `Basico`

### Requirement: Licencia de prueba (trial) para consultorio nuevo

Al alta de un consultorio nuevo, el sistema SHALL inicializar la licencia de prueba: `FechaVigenciaInicio=hoy`, `FechaVigenciaFin=hoy+60 días`, `EsPeriodoPrueba=1`, `Plan=Basico`, `Estatus=1`.

#### Scenario: Alta con trial

- GIVEN el registro de un consultorio nuevo
- WHEN se crea el consultorio
- THEN la licencia queda con 60 días de prueba desde hoy, `Plan=Basico` y `Estatus=1`

### Requirement: Licencia anual para consultorios existentes (backfill)

La migración SHALL backfillear los consultorios `DatosInstitucion.Id=1` (Dra. Tania) e `Id=2` (Consultorio Ivan): `EsPeriodoPrueba=0`, `FechaVigenciaInicio=hoy`, `FechaVigenciaFin=hoy+1 año`, `Plan=Basico`, `Estatus=1`.

#### Scenario: Backfill consultorios 1 y 2

- GIVEN la migración ejecutada sobre el clon SaaS
- WHEN se revisan los consultorios 1 y 2
- THEN tienen licencia anual vigente desde hoy por 1 año, `Plan=Basico`, `EsPeriodoPrueba=0`

### Requirement: Pantalla Licencia accesible a todos los usuarios

El sistema MUST exponer una pantalla `Licencia` accesible para TODOS los usuarios del consultorio (sin roles/permisos). MUST mostrar: plan, estatus, `FechaVigenciaInicio`, `FechaVigenciaFin`, datos bancarios, monto anual, carga de comprobante e historial de pagos. MUST ser accesible en modo normal y en modo restringido.

#### Scenario: Visualización

- GIVEN un usuario autenticado
- WHEN abre `Licencia`
- THEN ve plan, estatus, fechas, datos bancarios, monto y puede subir comprobante y ver historial

### Requirement: Plan Básico — monto fijo sin captura

El plan Básico SHALL costar $1,200.00 MXN anual. El sistema MUST NOT permitir capturar monto, MUST NOT incluir facturación/CFDI ni `RequiereFactura`. Intermedio y Avanzado SHALL modelarse solo como estado "en desarrollo", sin precio, funcionalidades ni upgrade.

#### Scenario: Monto no capturable

- GIVEN la pantalla Licencia
- WHEN el usuario sube un comprobante
- THEN no captura monto; el sistema usa el monto anual fijo del plan

### Requirement: Datos bancarios y monto desde configuración

El MVC SHALL leer de `Web.config` los datos bancarios: `PagoBanco=STP`, `PagoClabe=646180401600408914`, `PagoTitular=Ivan Francisco Bartolo Castro`, `PagoCorreoComprobante=contacto@desipr.com`, `PagoCorreoAviso=bartolocastro@gmail.com`, `PagoMontoAnual=1200`.

#### Scenario: Lectura de datos bancarios

- GIVEN el `Web.config` con las claves `Pago*`
- WHEN se renderiza la pantalla Licencia
- THEN muestra banco, CLABE, titular y monto anual desde configuración

### Requirement: Migración solo clon SaaS

La migración SHALL aplicarse solo al clon `db_9c7990_consultoriodesitst` (develop/main). MUST NOT tocar `-dermatologia` ni `DB_9C7990_Consultorio`.

#### Scenario: Aislamiento de entornos

- GIVEN el cambio implementado
- WHEN se revisan la migración y el código
- THEN no se afectan ramas ni BD de `-dermatologia`
