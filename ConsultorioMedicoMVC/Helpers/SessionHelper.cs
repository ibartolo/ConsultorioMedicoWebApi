using System;
using System.Web;
using System.Web.Security;
using ConsultorioMedicoEntities.Seguridad;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Helpers
{
    public class SessionHelper
    {
        public static bool ExisteSession()
        {
            var t = GetSessionUser();
            return t != null && t.Token != null && t.Token.ExpirationDate >= DateTime.Now;
        }

        public static void CloseSession()
        {
            FormsAuthentication.SignOut();
        }

        public static TokenCookie GetSessionUser()
        {
            if (HttpContext.Current?.User?.Identity is FormsIdentity fi && fi.Ticket != null)
                return JsonConvert.DeserializeObject<TokenCookie>(fi.Ticket.UserData);
            return null;
        }

        public static void CreateSession(string id)
        {
            var tokenCookie = JsonConvert.DeserializeObject<TokenCookie>(id);
            var cookie = FormsAuthentication.GetAuthCookie("token", true);
            cookie.Name = FormsAuthentication.FormsCookieName;
            cookie.Expires = tokenCookie.Token.ExpirationDate;

            var ticket = FormsAuthentication.Decrypt(cookie.Value);
            var newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate,
                cookie.Expires, ticket.IsPersistent, id);
            cookie.Value = FormsAuthentication.Encrypt(newTicket);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static DateTime GetDateCenterMexico()
        {
            return DateTime.UtcNow.AddHours(-6);
        }
    }
}
