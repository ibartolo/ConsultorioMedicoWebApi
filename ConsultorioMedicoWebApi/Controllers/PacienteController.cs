using DESI.ConsultorioMedico.Common.Mensajes;
using DESI.ConsultorioMedico.Paciente.Aplicacion;
using DESI.ConsultorioMedico.Paciente.Mensajes;
using Microsoft.AspNetCore.Mvc;

namespace DESI.ConsultorioMedico.WebApi.Controllers;

/// <summary>
/// Endpoints REST del dominio Paciente.
/// </summary>
[ApiController]
[Route("api/v1/paciente")]
public class PacienteController : ControllerBase
{
    private readonly IPacienteAplicacion _aplicacion;

    public PacienteController(IPacienteAplicacion aplicacion)
    {
        _aplicacion = aplicacion;
    }

    /// <summary>
    /// Obtiene todos los pacientes registrados.
    /// </summary>
    /// <response code="200">Lista de pacientes obtenida correctamente.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ModelResponse<GetPacientesResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ModelResponse<GetPacientesResponse>>> ObtenerTodos()
    {
        var result = await _aplicacion.ObtenerTodos();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un paciente por su identificador.
    /// </summary>
    /// <param name="id">Identificador del paciente.</param>
    /// <response code="200">Paciente encontrado.</response>
    /// <response code="400">Identificador inválido (menor o igual a cero).</response>
    /// <response code="404">Paciente no encontrado.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ModelResponse<PacienteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelResponse<PacienteResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ModelResponse<PacienteResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModelResponse<PacienteResponse>>> ObtenerPorId(int id)
    {
        if (id <= 0)
        {
            return BadRequest(ModelResponse<PacienteResponse>.Failure(
                SystemMessage.Get(SystemMessageTypes.IdentificadorInvalido)));
        }

        var result = await _aplicacion.ObtenerPorId(id);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Crea un paciente nuevo (Id = 0) o actualiza uno existente (Id mayor que cero).
    /// </summary>
    /// <param name="request">Datos del paciente a guardar.</param>
    /// <response code="200">Id del paciente creado o actualizado.</response>
    /// <response code="400">Datos de entrada inválidos.</response>
    [HttpPost("guardar")]
    [ProducesResponseType(typeof(ModelResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ModelResponse<int>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ModelResponse<int>>> Guardar([FromBody] GuardarPacienteRequest request)
    {
        if (!ModelState.IsValid)
        {
            var error = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? SystemMessage.Get(SystemMessageTypes.DatosEntradaInvalidos);

            return BadRequest(ModelResponse<int>.Failure(error));
        }

        var result = await _aplicacion.Guardar(request);
        return Ok(result);
    }
}
