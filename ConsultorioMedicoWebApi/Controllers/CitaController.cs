using System.Collections.Generic;
using System.Web.Http;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Cita")]
    public class CitaController : BaseController
    {
        private readonly CitaService _citaService;

        public CitaController()
        {
            _citaService = new CitaService();
        }

        [HttpGet, Route("List")]
        public ModelResponse<List<CitaDTO>> ObtenerCitas()
            => _citaService.ObtenerCitas(User.Identity.Name);

        [HttpGet, Route("{id:long}")]
        public ModelResponse<Cita> ObtenerCitaPorId(long id)
            => _citaService.ObtenerCitaPorId(id, User.Identity.Name);

        [HttpPost, Route("Guardar")]
        public ModelResponse<Cita> GuardarOActualizarCita(Cita cita)
            => _citaService.GuardarOActualizarCita(cita, User.Identity.Name);

        [HttpPost, Route("Eliminar/{id:int}")]
        public ModelResponse<bool> EliminarCita(int id)
            => _citaService.EliminarCita(id, User.Identity.Name);

        [HttpGet, Route("PorPaciente/{pacienteId:long}")]
        public ModelResponse<List<CitaDTO>> ObtenerCitasPorPaciente(long pacienteId)
            => _citaService.ObtenerCitasPorPaciente(pacienteId, User.Identity.Name);
    }
}
