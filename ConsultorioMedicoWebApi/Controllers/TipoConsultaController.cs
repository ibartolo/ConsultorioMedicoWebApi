using System.Collections.Generic;
using System.Web.Http;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/TipoConsulta")]
    public class TipoConsultaController : BaseController
    {
        private readonly TipoConsultaService _tipoConsultaService;

        public TipoConsultaController()
        {
            _tipoConsultaService = new TipoConsultaService();
        }

        [HttpGet, Route("List")]
        public ModelResponse<List<TipoConsulta>> ObtenerTiposConsulta()
            => _tipoConsultaService.ObtenerTiposConsulta(User.Identity.Name);
    }
}
