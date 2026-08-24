namespace DESI.ConsultorioMedico.Paciente.Mensajes;

/// <summary>
/// DTO de salida con la lista de pacientes.
/// </summary>
public class GetPacientesResponse
{
    public List<PacienteResponse> Pacientes { get; set; } = new();
}
