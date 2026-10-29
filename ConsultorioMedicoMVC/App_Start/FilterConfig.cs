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
            "Home.CambiarTema", "Home.Terminos", "Home.Privacidad"
        };

        public void OnAuthorization(AuthorizationContext ctx)
        {
            var key = ctx.ActionDescriptor.ControllerDescriptor.ControllerName + "." + ctx.ActionDescriptor.ActionName;
            if (PublicActions.Contains(key)) return;

            if (!SessionHelper.ExisteSession())
                ctx.Result = new RedirectToRouteResult(new RouteValueDictionary(
                    new { controller = "Home", action = "Autentication" }));
        }
    }
}
