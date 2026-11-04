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
            => _tipoConsultaService.ObtenerTiposConsulta(User.Identity.Name, EmpresaId);

        [HttpGet, Route("ListAdmin")]
        public ModelResponse<List<TipoConsulta>> ObtenerTiposConsultaAdmin()
            => _tipoConsultaService.ObtenerTiposConsultaAdmin(User.Identity.Name, EmpresaId);

        [HttpGet, Route("{id:long}")]
        public ModelResponse<TipoConsulta> ObtenerTipoConsultaPorId(long id)
            => _tipoConsultaService.ObtenerTipoConsultaPorId(id, User.Identity.Name, EmpresaId);

        [HttpPost, Route("Guardar")]
        public ModelResponse<TipoConsulta> GuardarOActualizarTipoConsulta(TipoConsulta tipoConsulta)
            => _tipoConsultaService.GuardarOActualizarTipoConsulta(tipoConsulta, User.Identity.Name, EmpresaId);

        [HttpPost, Route("Eliminar/{id:int}")]
        public ModelResponse<bool> EliminarTipoConsulta(int id)
            => _tipoConsultaService.EliminarTipoConsulta(id, User.Identity.Name, EmpresaId);
    }
}
