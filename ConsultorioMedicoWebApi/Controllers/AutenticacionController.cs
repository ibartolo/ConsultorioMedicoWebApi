using System.Web.Http;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/Autenticacion")]
    public class AutenticacionController : BaseController
    {
        private readonly AutenticacionService _autenticacionService;

        public AutenticacionController()
        {
            _autenticacionService = new AutenticacionService();
        }

        [HttpPost, Route("autenticar")]
        public ModelResponse<UsuarioDTO> Autenticar(Usuario request)
            => _autenticacionService.AutenticarUsuario(request?.UserName, request?.Password);
    }
}
