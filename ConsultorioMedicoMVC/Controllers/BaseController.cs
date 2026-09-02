using System.Collections.Generic;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;
using ConsultorioMedicoMVC.Helpers;

namespace ConsultorioMedicoMVC.Controllers
{
    public class BaseController : Controller
    {
        public HttpClientConnection httpClientConnection;
        public TokenCookie tokenCookie;
        public ModelResponse mr { get; set; }

        public BaseController()
        {
            httpClientConnection = new HttpClientConnection();
            mr = new ModelResponse();
            tokenCookie = SessionHelper.GetSessionUser();
        }

        public List<SelectListItem> MappingPropertiToDropDownList<T>(IEnumerable<T> items, string value, string title, string prefix = "")
        {
            var result = new List<SelectListItem>();
            if (items == null) return result;

            foreach (var item in items)
            {
                var v = typeof(T).GetProperty(value)?.GetValue(item, null);
                var t = typeof(T).GetProperty(title)?.GetValue(item, null);
                result.Add(new SelectListItem
                {
                    Value = v?.ToString(),
                    Text = prefix + (t?.ToString() ?? string.Empty)
                });
            }
            return result;
        }

        public string GenerarAvatarIniciales(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario)) return "?";
            var partes = nombreUsuario.Split(new[] { ' ', '.', '_' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length >= 2)
                return (partes[0][0].ToString() + partes[1][0].ToString()).ToUpper();
            return nombreUsuario.Substring(0, System.Math.Min(2, nombreUsuario.Length)).ToUpper();
        }
    }
}
