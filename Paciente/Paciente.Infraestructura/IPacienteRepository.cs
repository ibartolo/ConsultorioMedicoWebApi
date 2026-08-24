using DESI.ConsultorioMedico.Common.Mensajes;
using DESI.ConsultorioMedico.Paciente.Mensajes;

namespace DESI.ConsultorioMedico.Paciente.Infraestructura;

/// <summary>
/// Contrato del repositorio de Paciente. Vive en Infraestructura para abstraer
/// cualquier origen de datos (SQL hoy, HTTP en el futuro).
/// </summary>
public interface IPacienteRepository
{
    Task<List<PacienteData>> GetAllPacientes();
    Task<PacienteData?> GetPacienteById(int id);
    Task<OperationResult<int>> SaveOrUpdatePaciente(GuardarPacienteRequest request);
}
