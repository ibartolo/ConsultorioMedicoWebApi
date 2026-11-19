using System.Web.Http;
using ConsultorioMedicoEntities.Recuperacion;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    /// <summary>
    /// Recuperación de contraseña. Endpoints anónimos (sin token OAuth).
    /// </summary>
    [AllowAnonymous]
    [RoutePrefix("api/RecuperacionPassword")]
    public class RecuperacionPasswordController : BaseController
    {
        private readonly RecuperacionPasswordService _service;

        public RecuperacionPasswordController()
        {
            _service = new RecuperacionPasswordService();
        }

        [HttpPost, Route("Solicitar")]
        public ModelResponse Solicitar(SolicitarRecuperacionDTO dto)
            => _service.Solicitar(dto?.Email);

        [HttpGet, Route("Validar")]
        public ModelResponse Validar(string token)
            => _service.Validar(token);

        [HttpPost, Route("Restablecer")]
        public ModelResponse Restablecer(RestablecerPasswordDTO dto)
            => _service.Restablecer(dto?.Token, dto?.Password, dto?.Confirmacion);
    }
}
