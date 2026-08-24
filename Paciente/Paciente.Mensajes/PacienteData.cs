namespace DESI.ConsultorioMedico.Paciente.Mensajes;

/// <summary>
/// DTO crudo con las columnas del procedimiento almacenado (tabla dbo.Paciente).
/// </summary>
public class PacienteData
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string? ApellidoMaterno { get; set; }
    public string? Email { get; set; }
    public DateTime? FechaAlta { get; set; }
    public string? TelefonoCel { get; set; }
    public string? Comentarios { get; set; }
    public bool Estatus { get; set; }
    public int? Edad { get; set; }
    public string? Sexo { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? CreatedBY { get; set; }
    public DateTime? CreatedDT { get; set; }
    public string? UpdatedBY { get; set; }
    public DateTime? UpdatedDT { get; set; }
}
