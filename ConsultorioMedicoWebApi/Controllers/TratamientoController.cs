using System.Collections.Generic;
using System.Web.Http;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Tratamiento")]
    public class TratamientoController : BaseController
    {
        private readonly TratamientoService _tratamientoService;

        public TratamientoController()
        {
            _tratamientoService = new TratamientoService();
        }

        [HttpGet, Route("List")]
        public ModelResponse<List<Tratamiento>> ObtenerTratamientos()
            => _tratamientoService.ObtenerTratamientos(User.Identity.Name);

        [HttpGet, Route("{id:long}")]
        public ModelResponse<Tratamiento> ObtenerTratamientoPorId(long id)
            => _tratamientoService.ObtenerTratamientoPorId(id, User.Identity.Name);

        [HttpPost, Route("Guardar")]
        public ModelResponse<Tratamiento> GuardarOActualizarTratamiento(Tratamiento tratamiento)
            => _tratamientoService.GuardarOActualizarTratamiento(tratamiento, User.Identity.Name);
    }
}
