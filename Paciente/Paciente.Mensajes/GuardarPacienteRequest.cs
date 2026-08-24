using System.ComponentModel.DataAnnotations;

namespace DESI.ConsultorioMedico.Paciente.Mensajes;

/// <summary>
/// DTO de entrada para crear o actualizar un paciente.
/// </summary>
public class GuardarPacienteRequest
{
    /// <summary>0 para crear; mayor que 0 para actualizar.</summary>
    public int Id { get; set; }

    [Required(ErrorMessage = "Nombre es requerido")]
    [MaxLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "ApellidoPaterno es requerido")]
    [MaxLength(50, ErrorMessage = "El apellido paterno no puede exceder 50 caracteres.")]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "El apellido materno no puede exceder 50 caracteres.")]
    public string? ApellidoMaterno { get; set; }

    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    [MaxLength(50, ErrorMessage = "El email no puede exceder 50 caracteres.")]
    public string? Email { get; set; }

    [MaxLength(100, ErrorMessage = "El teléfono no puede exceder 100 caracteres.")]
    public string? TelefonoCel { get; set; }

    [MaxLength(1000, ErrorMessage = "Los comentarios no pueden exceder 1000 caracteres.")]
    public string? Comentarios { get; set; }

    [MaxLength(15, ErrorMessage = "El sexo no puede exceder 15 caracteres.")]
    [RegularExpression("^(Masculino|Femenino)$", ErrorMessage = "El sexo debe ser 'Masculino' o 'Femenino'.")]
    public string? Sexo { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    [Required(ErrorMessage = "Estatus es requerido")]
    public bool Estatus { get; set; }
}
