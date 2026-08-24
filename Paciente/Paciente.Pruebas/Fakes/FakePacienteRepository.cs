using DESI.ConsultorioMedico.Common.Mensajes;
using DESI.ConsultorioMedico.Paciente.Infraestructura;
using DESI.ConsultorioMedico.Paciente.Mensajes;

namespace DESI.ConsultorioMedico.Paciente.Pruebas.Fakes;

/// <summary>
/// Implementación falsa del repositorio para pruebas unitarias (sin base de datos real).
/// </summary>
public class FakePacienteRepository : IPacienteRepository
{
    private readonly List<PacienteData> _datos;

    /// <summary>
    /// Cantidad de veces que se invocó <see cref="SaveOrUpdatePaciente"/>.
    /// </summary>
    public int GuardadoCount { get; private set; }

    public FakePacienteRepository(List<PacienteData>? datos = null)
    {
        _datos = datos ?? new List<PacienteData>();
    }

    public Task<List<PacienteData>> GetAllPacientes() => Task.FromResult(_datos);

    public Task<PacienteData?> GetPacienteById(int id)
        => Task.FromResult(_datos.FirstOrDefault(p => p.Id == id));

    public Task<OperationResult<int>> SaveOrUpdatePaciente(GuardarPacienteRequest request)
    {
        GuardadoCount++;
        var id = request.Id == 0 ? 1 : request.Id;
        return Task.FromResult(OperationResult<int>.Success(id));
    }
}
