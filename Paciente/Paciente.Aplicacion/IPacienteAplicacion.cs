using DESI.ConsultorioMedico.Common.Mensajes;
using DESI.ConsultorioMedico.Paciente.Mensajes;

namespace DESI.ConsultorioMedico.Paciente.Aplicacion;

/// <summary>
/// Contrato de la capa de aplicación para el dominio Paciente.
/// </summary>
public interface IPacienteAplicacion
{
    Task<ModelResponse<GetPacientesResponse>> ObtenerTodos();
    Task<ModelResponse<PacienteResponse>> ObtenerPorId(int id);
    Task<ModelResponse<int>> Guardar(GuardarPacienteRequest request);
}
