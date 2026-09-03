using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class UsuarioController : BaseController
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController() : base()
        {
            _usuarioService = new UsuarioService(httpClientConnection);
        }

        #region Views
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Nuevo(long id = 0)
        {
            if (id > 0)
            {
                var usuario = await _usuarioService.ObtenerUsuarioPorId(id);
                return View(usuario ?? new Usuario());
            }
            return View(new Usuario());
        }
        #endregion

        #region Data Access
        public async Task<string> ConsultarTodosUsuarios()
        {
            var response = await _usuarioService.ConsultarTodosUsuarios();
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        public async Task<string> GuardarOActualizarUsuario(Usuario usuario)
        {
            var response = await _usuarioService.GuardarOActualizarUsuario(usuario);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}
