namespace DESI.ConsultorioMedico.Paciente.Mensajes;

/// <summary>
/// DTO de salida con los datos de un paciente.
/// </summary>
public class PacienteResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string? ApellidoMaterno { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Sexo { get; set; }
    public int Edad { get; set; }
    public string? TelefonoCel { get; set; }
    public string? Email { get; set; }
    public bool Estatus { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Comentarios { get; set; }
}
