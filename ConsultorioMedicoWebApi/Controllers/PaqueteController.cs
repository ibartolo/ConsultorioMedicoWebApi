using System.Collections.Generic;
using System.Web.Http;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Paquete")]
    public class PaqueteController : BaseController
    {
        private readonly PaqueteService _paqueteService;

        public PaqueteController()
        {
            _paqueteService = new PaqueteService();
        }

        [HttpGet, Route("List")]
        public ModelResponse<List<Paquete>> ObtenerPaquetes()
            => _paqueteService.ObtenerPaquetes(User.Identity.Name);

        [HttpGet, Route("{id:long}")]
        public ModelResponse<Paquete> ObtenerPaquetePorId(long id)
            => _paqueteService.ObtenerPaquetePorId(id, User.Identity.Name);

        [HttpPost, Route("Guardar")]
        public ModelResponse<Paquete> GuardarOActualizarPaquete(Paquete paquete)
            => _paqueteService.GuardarOActualizarPaquete(paquete, User.Identity.Name);
    }
}
