using DESI.ConsultorioMedico.Common.Mensajes;
using DESI.ConsultorioMedico.Paciente.Infraestructura;
using DESI.ConsultorioMedico.Paciente.Mensajes;

namespace DESI.ConsultorioMedico.Paciente.Aplicacion;

/// <summary>
/// Orquesta el flujo Paciente: repositorio → mapeo → sobre público.
/// </summary>
public class PacienteAplicacion : IPacienteAplicacion
{
    private readonly IPacienteRepository _repository;

    public PacienteAplicacion(IPacienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<ModelResponse<GetPacientesResponse>> ObtenerTodos()
    {
        var data = await _repository.GetAllPacientes();
        var pacientes = Mapp.MapToPacientes(data);
        var response = Mapp.MapToPacientesResponse(pacientes);

        return ModelResponse<GetPacientesResponse>.Success(
            response,
            SystemMessage.Get(SystemMessageTypes.PacientesObtenidos));
    }

    public async Task<ModelResponse<PacienteResponse>> ObtenerPorId(int id)
    {
        var data = await _repository.GetPacienteById(id);
        if (data is null)
        {
            return ModelResponse<PacienteResponse>.Failure(
                SystemMessage.Get(SystemMessageTypes.PacienteNoEncontrado));
        }

        var paciente = Mapp.MapToPaciente(data);
        var response = Mapp.MapToPacienteResponse(paciente);

        return ModelResponse<PacienteResponse>.Success(
            response,
            SystemMessage.Get(SystemMessageTypes.PacientesObtenidos));
    }

    public async Task<ModelResponse<int>> Guardar(GuardarPacienteRequest request)
    {
        var resultado = await _repository.SaveOrUpdatePaciente(request);

        if (!resultado.IsSuccess)
        {
            return ModelResponse<int>.Failure(resultado.Message);
        }

        var mensaje = request.Id == 0
            ? SystemMessage.Get(SystemMessageTypes.PacienteCreado)
            : SystemMessage.Get(SystemMessageTypes.PacienteActualizado);

        return ModelResponse<int>.Success(resultado.Data, mensaje);
    }
}
