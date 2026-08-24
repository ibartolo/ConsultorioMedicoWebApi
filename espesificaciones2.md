## 12. DOMINIO PACIENTE - ENTIDADES Y VALUE OBJECTS

### 12.1. Entidad Paciente (basada en tabla dbo.Paciente)
| Campo | Tipo en BD | Propiedad en C# | Descripción |
|-------|------------|-----------------|-------------|
| Id | int (PK) | int | Identificador único |
| Nombre | nvarchar(50) | string | Nombre del paciente |
| ApellidoPaterno | nvarchar(50) | string | Apellido paterno |
| ApellidoMaterno | nvarchar(50) | string? | Apellido materno (nullable) |
| Email | nvarchar(50) | string? | Correo electrónico |
| FechaAlta | datetime | DateTime? | Fecha de alta |
| TelefonoCel | nvarchar(100) | string? | Teléfono celular |
| Comentarios | nvarchar(1000) | string? | Observaciones |
| Estatus | bit | bool | Activo (1) / Inactivo (0) |
| Edad | int | int? | Edad del paciente |
| Sexo | nvarchar(15) | string? | Masculino/Femenino |
| FechaNacimiento | datetime | DateTime? | Fecha de nacimiento |
| CreatedBY | nvarchar(25) | string? | Usuario que creó |
| CreatedDT | datetime | DateTime? | Fecha de creación |
| UpdatedBY | nvarchar(25) | string? | Usuario que modificó |
| UpdatedDT | datetime | DateTime? | Fecha de modificación |

### 12.2. ValueObjects
- **Sexo**: enum { Masculino, Femenino } (mapeo desde columna Sexo)
- **Email**: string con validación de formato (desde columna Email)
- **Telefono**: string con validación de formato (desde columna TelefonoCel)

---

## 13. DOMINIO PACIENTE - REQUESTS Y RESPONSES

### 13.1. GuardarPacienteRequest (para creación y actualización)
| Campo | Tipo | Obligatorio | Descripción |
|-------|------|-------------|-------------|
| Id | int | Sí | 0 = nuevo, > 0 = actualización |
| Nombre | string | Sí | Máximo 50 caracteres |
| ApellidoPaterno | string | Sí | Máximo 50 caracteres |
| ApellidoMaterno | string | No | Máximo 50 caracteres |
| Email | string | No | Formato válido de email |
| TelefonoCel | string | No | Máximo 100 caracteres |
| Comentarios | string | No | Máximo 1000 caracteres |
| Sexo | string | No | "Masculino" o "Femenino" |
| FechaNacimiento | DateTime? | No | Formato fecha |
| Edad | int? | No | Edad calculada |
| Estatus | bool | Sí | Activo/Inactivo |

### 13.2. PacienteResponse
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | int | Identificador |
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

## 14. DOMINIO PACIENTE - FIRMAS DE SPS

### 14.1. GetAllPacientes
**Parámetros:** Ninguno
**Retorno:** Todas las columnas de la tabla Paciente

### 14.2. GetPacienteById
**Parámetros:** @Id int
**Retorno:** Todas las columnas de la tabla Paciente

### 14.3. SaveOrUpdatePaciente (15 parámetros)
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

## 15. DOMINIO PACIENTE - ENDPOINTS

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

## 16. DOMINIO PACIENTE - VALIDACIONES

### 16.1. DataAnnotations por campo
| Campo | Validación |
|-------|------------|
| Nombre | [Required(ErrorMessage = "Nombre es requerido")], [MaxLength(50)] |
| ApellidoPaterno | [Required(ErrorMessage = "Apellido paterno es requerido")], [MaxLength(50)] |
| ApellidoMaterno | [MaxLength(50)] |
| Email | [EmailAddress(ErrorMessage = "Email inválido")], [MaxLength(50)] |
| TelefonoCel | [MaxLength(100)] |
| Comentarios | [MaxLength(1000)] |
| Sexo | [MaxLength(15)] |
| Estatus | [Required] |

### 16.2. Reglas de negocio
- Nombre: obligatorio, máximo 50 caracteres.
- ApellidoPaterno: obligatorio, máximo 50 caracteres.
- Email: formato válido si se proporciona.
- Estatus: por defecto true (activo) al crear.
- Edad: se calcula desde FechaNacimiento si se proporciona.