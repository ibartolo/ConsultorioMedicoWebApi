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

        // Tenant SIEMPRE del claim (@ConsultorioID = EmpresaId). Nunca del cliente.
        [HttpGet, Route("List")]
        public ModelResponse<List<DatosFiscales>> ObtenerDatosFiscales()
            => _datosFiscalesService.ObtenerDatosFiscales(EmpresaId, User.Identity.Name);

        // Fila fiscal del tenant actual (1:1). Reemplaza al antiguo "Unico" global.
        // Si el tenant no tiene fila fiscal (p.ej. consultorio 1), devuelve IsSuccess=false.
        [HttpGet, Route("Actual")]
        public ModelResponse<DatosFiscales> ObtenerDatosFiscalActual()
            => _datosFiscalesService.ObtenerDatosFiscalActual(EmpresaId, User.Identity.Name);

        [HttpGet, Route("{id:int}")]
        public ModelResponse<DatosFiscales> ObtenerDatosFiscalPorId(int id)
            => _datosFiscalesService.ObtenerDatosFiscalPorId(id, EmpresaId, User.Identity.Name);

        [HttpPost, Route("Guardar")]
        public ModelResponse<DatosFiscales> GuardarDatosFiscal(DatosFiscales datos)
            => _datosFiscalesService.GuardarDatosFiscal(datos, EmpresaId, User.Identity.Name);

        [HttpPost, Route("Eliminar/{id:int}")]
        public ModelResponse<bool> EliminarDatosFiscal(int id)
            => _datosFiscalesService.EliminarDatosFiscal(id, EmpresaId, User.Identity.Name);
    }
}
