using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using ConsultorioMedicoMVC.Helpers;

namespace ConsultorioMedicoMVC.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new AuthenticationFilter());
        }
    }

    public class AuthenticationFilter : IAuthorizationFilter
    {
        private static readonly HashSet<string> PublicActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Home.Autentication", "Home.LogIn", "Home.LogOut",
            "Home.CambiarTema", "Home.Terminos", "Home.Privacidad",
            "Registro.Index", "Registro.Guardar",
            "RecuperacionPassword.Solicitar", "RecuperacionPassword.Restablecer", "RecuperacionPassword.Guardar"
        };

        // Acciones permitidas en modo restringido (licencia vencida). Sin DatosInstitucion.Branding.
        private static readonly HashSet<string> AccionesModoRestringido = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Licencia.Index", "Licencia.Consultar", "Licencia.ConsultarPagos",
            "Licencia.SubirComprobante", "Licencia.Descargar",
            "Home.LogOut"
        };

        public void OnAuthorization(AuthorizationContext ctx)
        {
            var key = ctx.ActionDescriptor.ControllerDescriptor.ControllerName + "." + ctx.ActionDescriptor.ActionName;
            if (PublicActions.Contains(key)) return;

            if (!SessionHelper.ExisteSession())
            {
                ctx.Result = new RedirectToRouteResult(new RouteValueDictionary(
                    new { controller = "Home", action = "Autentication" }));
                return;
            }

            // Modo restringido por licencia vencida: confinar a Licencia/LogOut.
            // Cookies previas sin el flag (VigenciaVencida = false) no restringen.
            var tokenCookie = SessionHelper.GetSessionUser();
            if (tokenCookie != null && tokenCookie.VigenciaVencida && !AccionesModoRestringido.Contains(key))
            {
                if (ctx.HttpContext.Request.IsAjaxRequest())
                {
                    // AJAX devuelve JSON, no HTML de redirect (evita vistas rotas).
                    ctx.Result = new JsonResult
                    {
                        Data = new { IsSuccess = false, Message = "Licencia vencida — modo restringido" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    ctx.Result = new RedirectToRouteResult(new RouteValueDictionary(
                        new { controller = "Licencia", action = "Index" }));
                }
            }
        }
    }
}
