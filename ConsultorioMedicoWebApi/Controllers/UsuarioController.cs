using System.Collections.Generic;
using System.Web.Http;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Usuario")]
    public class UsuarioController : BaseController
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController()
        {
            _usuarioService = new UsuarioService();
        }

        [HttpGet, Route("List")]
        public ModelResponse<List<Usuario>> ObtenerUsuarios()
            => _usuarioService.ObtenerUsuarios(User.Identity.Name);

        [HttpGet, Route("{id:long}")]
        public ModelResponse<Usuario> ObtenerUsuarioPorId(long id)
            => _usuarioService.ObtenerUsuarioPorId(id, User.Identity.Name);

        [HttpPost, Route("Guardar")]
        public ModelResponse<Usuario> GuardarOActualizarUsuario(Usuario usuario)
            => _usuarioService.GuardarOActualizarUsuario(usuario, User.Identity.Name);
    }
}
