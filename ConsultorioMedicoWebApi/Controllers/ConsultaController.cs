using System.Collections.Generic;
using System.Web.Http;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Consulta")]
    public class ConsultaController : BaseController
    {
        private readonly ConsultaService _consultaService;

        public ConsultaController()
        {
            _consultaService = new ConsultaService();
        }

        [HttpGet, Route("List")]
        public ModelResponse<List<ConsultaDTO>> ObtenerConsultas()
            => _consultaService.ObtenerConsultas(User.Identity.Name);

        [HttpGet, Route("{id:long}")]
        public ModelResponse<Consulta> ObtenerConsultaPorId(long id)
            => _consultaService.ObtenerConsultaPorId(id, User.Identity.Name);

        [HttpPost, Route("Guardar")]
        public ModelResponse<Consulta> GuardarOActualizarConsulta(Consulta consulta)
            => _consultaService.GuardarOActualizarConsulta(consulta, User.Identity.Name);

        [HttpGet, Route("Receta/{consultaId:long}")]
        public ModelResponse<RecetaDTO> ObtenerRecetaPorConsulta(long consultaId, int institucionId = 1)
            => _consultaService.ObtenerRecetaPorConsulta(consultaId, institucionId, User.Identity.Name);

        [HttpGet, Route("PorPaciente/{pacienteId:long}")]
        public ModelResponse<List<ConsultaDTO>> ObtenerConsultasPorPaciente(long pacienteId)
            => _consultaService.ObtenerConsultasPorPaciente(pacienteId, User.Identity.Name);
    }
}
