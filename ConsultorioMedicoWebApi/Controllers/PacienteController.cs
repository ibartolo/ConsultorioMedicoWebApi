using System.Collections.Generic;
using System.Web.Http;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Paciente")]
    public class PacienteController : BaseController
    {
        private readonly PacienteService _pacienteService;

        public PacienteController()
        {
            _pacienteService = new PacienteService();
        }

        [HttpGet, Route("List")]
        public ModelResponse<List<PacienteDTO>> ObtenerPacientes()
            => _pacienteService.ObtenerPacientes(User.Identity.Name);

        [HttpGet, Route("{id:long}")]
        public ModelResponse<Paciente> ObtenerPacientePorId(long id)
            => _pacienteService.ObtenerPacientePorId(id, User.Identity.Name);

        [HttpPost, Route("Guardar")]
        public ModelResponse<Paciente> GuardarOActualizarPaciente(Paciente paciente)
            => _pacienteService.GuardarOActualizarPaciente(paciente, User.Identity.Name);

        [HttpPost, Route("Eliminar/{id:int}")]
        public ModelResponse<bool> EliminarPaciente(int id)
            => _pacienteService.EliminarPaciente(id, User.Identity.Name);
    }
}
