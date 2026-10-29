using System.Collections.Generic;
using System.Web.Http;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/DatosFiscales")]
    public class DatosFiscalesController : BaseController
    {
        private readonly DatosFiscalesService _datosFiscalesService;

        public DatosFiscalesController()
        {
            _datosFiscalesService = new DatosFiscalesService();
        }

        [HttpGet, Route("List")]
        public ModelResponse<List<DatosFiscales>> ObtenerDatosFiscales()
            => _datosFiscalesService.ObtenerDatosFiscales(User.Identity.Name);

        [HttpGet, Route("Unico")]
        public ModelResponse<DatosFiscales> ObtenerDatosFiscalUnico()
            => _datosFiscalesService.ObtenerDatosFiscalUnico(User.Identity.Name);

        [HttpGet, Route("{id:int}")]
        public ModelResponse<DatosFiscales> ObtenerDatosFiscalPorId(int id)
            => _datosFiscalesService.ObtenerDatosFiscalPorId(id, User.Identity.Name);

        [HttpPost, Route("Guardar")]
        public ModelResponse<DatosFiscales> GuardarDatosFiscal(DatosFiscales datos)
            => _datosFiscalesService.GuardarDatosFiscal(datos, User.Identity.Name);

        [HttpPost, Route("Eliminar/{id:int}")]
        public ModelResponse<bool> EliminarDatosFiscal(int id)
            => _datosFiscalesService.EliminarDatosFiscal(id, User.Identity.Name);
    }
}
