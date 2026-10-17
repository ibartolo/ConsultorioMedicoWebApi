using System.Collections.Generic;
using System.Web.Http;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Historial")]
    public class HistorialController : BaseController
    {
        private readonly HistorialService _historialService;

        public HistorialController()
        {
            _historialService = new HistorialService();
        }

        [HttpGet, Route("TratamientosPorPaciente/{pacienteId:long}")]
        public ModelResponse<List<TratamientoHistoricoDTO>> ObtenerTratamientosPorPaciente(long pacienteId)
            => _historialService.ObtenerTratamientosPorPaciente(pacienteId, User.Identity.Name);
    }
}
