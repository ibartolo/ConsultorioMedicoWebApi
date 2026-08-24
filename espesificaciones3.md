## 13. DOMINIO PACIENTE - REQUESTS Y RESPONSES (ACTUALIZADO)

### 13.1. GuardarPacienteRequest
| Campo | Tipo | Obligatorio | Descripción |
|-------|------|-------------|-------------|
| Id | int | Sí | 0 = nuevo, > 0 = actualización |
| Nombre | string | Sí | Máximo 50 caracteres |
| ApellidoPaterno | string | Sí | Máximo 50 caracteres |
| ApellidoMaterno | string | No | Máximo 50 caracteres |
| Email | string | No | Formato válido de email |
| TelefonoCel | string | No | Máximo 100 caracteres |
| Comentarios | string | No | Máximo 1000 caracteres |
| Sexo | string | No | "Masculino" o "Femenino" (validado) |
| FechaNacimiento | DateTime? | No | Formato fecha |
| Estatus | bool | Sí | Activo/Inactivo |

**Nota:** El campo `Edad` **no** se incluye en el Request. Se calcula automáticamente en el SP.

### 13.2. PacienteResponse (Actualizado)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | int | Identificador |
| Nombre | string | Nombre individual |
| ApellidoPaterno | string | Apellido paterno |
| ApellidoMaterno | string? | Apellido materno |
| NombreCompleto | string | Nombre + ApellidoPaterno + ApellidoMaterno |
| Sexo | string | Masculino/Femenino |
| Edad | int | Edad calculada |
| TelefonoCel | string | Teléfono de contacto |
| Email | string | Correo electrónico |
| Estatus | bool | Activo/Inactivo |
| FechaNacimiento | DateTime? | Fecha de nacimiento |
| Comentarios | string | Observaciones |

### 13.3. GetPacientesResponse
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Pacientes | List<PacienteResponse> | Lista de pacientes |
| Result | OperationResult | Resultado de la operación |

---

## 14. DOMINIO PACIENTE - FIRMAS DE SPS (ACTUALIZADO)

**Nota:** En esta fase solo se implementarán **3 procedimientos** del dominio Paciente. Los SPs de filtrado (`GetPacientesFiltrados`, `GetAllPacienteByEstatus`, `GetPacienteByFechaNacimiento`) **no serán utilizados** en esta fase y quedarán pendientes para futuras iteraciones.

### 14.1. GetAllPacientes
**Parámetros:** Ninguno
**Retorno:** Todas las columnas de la tabla Paciente

### 14.2. GetPacienteById
**Parámetros:** @Id int
**Retorno:** Todas las columnas de la tabla Paciente

### 14.3. SaveOrUpdatePaciente (16 parámetros)
| # | Parámetro | Tipo en BD | Tipo en C# |
|---|-----------|------------|------------|
| 1 | @Id | int | int |
| 2 | @Nombre | nvarchar(50) | string |
| 3 | @ApellidoPaterno | nvarchar(50) | string |
| 4 | @ApellidoMaterno | nvarchar(50) | string? |
| 5 | @Email | nvarchar(50) | string? |
| 6 | @FechaAlta | datetime | DateTime? |
| 7 | @TelefonoCel | nvarchar(100) | string? |
| 8 | @Comentarios | nvarchar(1000) | string? |
| 9 | @Estatus | bit | bool |
| 10 | @Edad | int | int? |
| 11 | @Sexo | nvarchar(15) | string? |
| 12 | @FechaNacimiento | datetime | DateTime? |
| 13 | @CreatedBY | nvarchar(25) | string? |
| 14 | @CreatedDT | datetime | DateTime? |
| 15 | @UpdatedBY | nvarchar(25) | string? |
| 16 | @UpdatedDT | datetime | DateTime? |

**Retorno:** Id del paciente insertado/actualizado

---

## 15. DOMINIO PACIENTE - ENDPOINTS (ACTUALIZADO)

### 15.1. Obtener todos los pacientes
**GET** `/api/v1/paciente`
**Controller:** PacienteController
**Aplicacion:** PacienteAplicacion.ObtenerTodos()
**SP:** GetAllPacientes
**Response:** GetPacientesResponse

### 15.2. Obtener paciente por ID
**GET** `/api/v1/paciente/{id}`
**Controller:** PacienteController
**Aplicacion:** PacienteAplicacion.ObtenerPorId()
**SP:** GetPacienteById
**Response:** ModelResponse<PacienteResponse>

### 15.3. Guardar o actualizar paciente
**POST** `/api/v1/paciente/guardar`
**Body:** GuardarPacienteRequest
**Controller:** PacienteController
**Aplicacion:** PacienteAplicacion.Guardar()
**SP:** SaveOrUpdatePaciente
**Response:** ModelResponse<int> (devuelve el Id del paciente)

---

## 17. ACLARACIONES DE DISEÑO PARA EL DOMINIO PACIENTE

### 17.1. Sexo (ValueObject vs String)
**Decisión:** Se manejará como **string** en el Request y en la capa de Infraestructura. La aplicación validará que el valor sea "Masculino" o "Femenino" mediante DataAnnotations. No se implementará como enum para evitar conversiones complejas con la BD.

### 17.2. Edad
**Decisión:** La **Edad** se **calcula automáticamente** en el SP `SaveOrUpdatePaciente` a partir de la `FechaNacimiento`. No se recibe como parámetro de entrada en el Request. El campo `Edad` en la tabla se actualiza solo desde el SP.

### 17.3. Campos de Auditoría (CreatedBY, CreatedDT, UpdatedBY, UpdatedDT)
**Decisión:** Estos campos los llena **el SP** (`SaveOrUpdatePaciente`) automáticamente:
- **CreatedBY** y **CreatedDT** se llenan solo cuando es un nuevo registro (Id = 0).
- **UpdatedBY** y **UpdatedDT** se llenan siempre que se actualiza (Id > 0).
- La aplicación no los envía en el Request ni los manipula.

### 17.4. PacienteResponse
**Decisión:** Se incluirán los campos individuales (`Nombre`, `ApellidoPaterno`, `ApellidoMaterno`) además de `NombreCompleto` para facilitar su uso en formularios de edición.