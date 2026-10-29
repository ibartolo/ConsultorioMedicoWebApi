using System;
using System.Threading.Tasks;
using System.Web;
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
            var tema = Request.Cookies["tema"] != null ? Request.Cookies["tema"].Value : null;
            if (tema != "claro" && tema != "oscuro")
            {
                tema = "claro";
                Response.Cookies.Set(new HttpCookie("tema", tema)
                {
                    Expires = DateTime.Now.AddYears(1),
                    Path = "/",
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });
            }

            ViewBag.Tema = tema;
            ViewBag.UsuarioRecordado = Request.Cookies["recordarUsuario"] != null
                ? Request.Cookies["recordarUsuario"].Value
                : string.Empty;

            return View();
        }

        // GET: Home/Manual (solo usuarios autenticados)
        public ActionResult Manual()
        {
            if (!SessionHelper.ExisteSession())
                return RedirectToAction("Autentication");

            return View();
        }

        // GET: Home/Terminos y Home/Privacidad (públicos)
        public ActionResult Terminos()
        {
            return View();
        }

        public ActionResult Privacidad()
        {
            return View();
        }

        // POST: Home/CambiarTema (público: usable desde login y configuración)
        [HttpPost]
        public string CambiarTema(string tema)
        {
            if (tema != "oscuro") tema = "claro";

            Response.Cookies.Set(new HttpCookie("tema", tema)
            {
                Expires = DateTime.Now.AddYears(1),
                Path = "/",
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            });

            return JsonConvert.SerializeObject(new { IsSuccess = true, Tema = tema, Message = "Tema actualizado." });
        }

        // POST: Home/LogIn (doble encode: devuelve JSON como string)
        [HttpPost]
        public async Task<string> LogIn(string user, string pass, bool recordar = false)
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

                if (recordar)
                {
                    Response.Cookies.Set(new HttpCookie("recordarUsuario", user)
                    {
                        Expires = DateTime.Now.AddYears(1),
                        Path = "/",
                        HttpOnly = true,
                        SameSite = SameSiteMode.Lax
                    });
                }
                else
                {
                    Response.Cookies.Set(new HttpCookie("recordarUsuario", "")
                    {
                        Expires = DateTime.Now.AddDays(-1),
                        Path = "/",
                        HttpOnly = true,
                        SameSite = SameSiteMode.Lax
                    });
                }

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
