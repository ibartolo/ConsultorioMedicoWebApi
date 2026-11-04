# Especificación: CRUD de TipoConsulta

## Purpose

CRUD completo de `TipoConsulta` por consultorio (hoy solo existe `GET List`). Campos: `Nombre` (requerido), `Duracion`, `Descripcion`, `Estatus`. Soft delete con `Estatus`. Sin seeds.

## Requirements

### Requirement: Listado por consultorio

El sistema SHALL listar los `TipoConsulta` del consultorio autenticado, filtrando por `ConsultorioID`.

#### Scenario: Listar tipos de consulta

- GIVEN tipos de consulta de los consultorios 1 y 2
- WHEN el usuario del consultorio 1 lista
- THEN ve solo los de su consultorio

### Requirement: Crear TipoConsulta

El sistema SHALL permitir crear un `TipoConsulta` con `Nombre` requerido y `Duracion`/`Descripcion` opcionales, asignando el `ConsultorioID` actual y `Estatus` activo.

#### Scenario: Creación válida

- GIVEN `Nombre` con valor
- WHEN se guarda
- THEN se crea el tipo con `ConsultorioID` del tenant y `Estatus=1`

#### Scenario: Nombre vacío

- GIVEN `Nombre` vacío
- WHEN se guarda
- THEN se rechaza por validación

### Requirement: Editar TipoConsulta

El sistema SHALL permitir editar `Nombre`, `Duracion` y `Descripcion` de un `TipoConsulta` del propio consultorio.

#### Scenario: Edición válida

- GIVEN un TipoConsulta del consultorio
- WHEN se editan sus campos y se guarda
- THEN se actualiza

#### Scenario: TipoConsulta ajeno

- GIVEN un `Id` de otro consultorio
- WHEN se intenta editar
- THEN responde `IsSuccess=false` sin modificar

### Requirement: Desactivar TipoConsulta (soft delete)

El sistema SHALL permitir desactivar un `TipoConsulta` mediante baja lógica (`Estatus=0`), sin borrado físico.

#### Scenario: Desactivación

- GIVEN un TipoConsulta activo
- WHEN se desactiva
- THEN `Estatus=0` y desaparece del listado activo

### Requirement: Sin seeds

Un consultorio nuevo SHALL iniciar con `TipoConsulta` vacío; el sistema MUST NOT sembrar tipos por defecto.

#### Scenario: Catálogo vacío al crear consultorio

- GIVEN un consultorio recién creado
- WHEN abre el catálogo de TipoConsulta
- THEN la lista está vacía
