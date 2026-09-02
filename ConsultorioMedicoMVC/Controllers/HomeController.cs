using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.Helpers;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;
using Serilog;

namespace ConsultorioMedicoMVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly AutenticacionService _autenticacionService;

        public HomeController() : base()
        {
            _autenticacionService = new AutenticacionService(httpClientConnection);
        }

        // GET: Home/Autentication (login, allow-list)
        public ActionResult Autentication()
        {
            return View();
        }

        // POST: Home/LogIn (doble encode: devuelve JSON como string)
        [HttpPost]
        public async Task<string> LogIn(string user, string pass)
        {
            var mr = new ModelResponse();
            try
            {
                // PASO 1: autenticar (sin token) para obtener datos del usuario
                var response = await _autenticacionService.AutenticarUsuario(new Usuario { UserName = user, Password = pass });
                if (!response.IsSuccess || response.Response == null)
                    return JsonConvert.SerializeObject(new ModelResponse
                    {
                        IsSuccess = false,
                        Message = response?.Message ?? "Usuario o contraseña incorrectos"
                    });

                // PASO 2: obtener token OAuth (password grant)
                Token token = await httpClientConnection.GetToken(user, pass);
                if (token == null)
                    return JsonConvert.SerializeObject(new ModelResponse { IsSuccess = false, Message = "Error de usuario o contraseña" });

                token.ExpirationDate = DateTime.Now.AddSeconds(token.expires_in);
                var usuario = response.Response;

                var tokenCookie = new TokenCookie
                {
                    Token = token,
                    UserID = usuario.Id,
                    EmpresaID = usuario.CompaniaID ?? 0,
                    UserName = user,
                    ProfileImage = null,
                    UserAvatar = GenerarAvatarIniciales(user)
                };
                SessionHelper.CreateSession(JsonConvert.SerializeObject(tokenCookie));

                mr.IsSuccess = true;
                mr.Message = "Ok";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LogIn error");
                mr.IsSuccess = false;
                mr.Message = ex.Message;
            }
            return JsonConvert.SerializeObject(mr);
        }

        // GET: Home/Index
        public ActionResult Index()
        {
            if (!SessionHelper.ExisteSession())
                return RedirectToAction("Autentication");

            ViewBag.UserName = SessionHelper.GetSessionUser()?.UserName;
            return View();
        }

        // GET: Home/LogOut
        public ActionResult LogOut()
        {
            SessionHelper.CloseSession();
            return RedirectToAction("Autentication");
        }
    }
}
