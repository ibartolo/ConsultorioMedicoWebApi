using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class DatosInstitucionController : BaseController
    {
        private readonly DatosInstitucionService _datosInstitucionService;

        public DatosInstitucionController() : base()
        {
            _datosInstitucionService = new DatosInstitucionService(httpClientConnection);
        }

        public ActionResult Index()
        {
            ViewBag.InstitucionId = TenantEmpresaId;
            return View();
        }

        // El id de la institución es el tenant: se resuelve de la cookie, nunca del cliente.
        public async Task<string> Consultar()
        {
            var response = await _datosInstitucionService.ConsultarDatosInstitucion();
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: branding (nombre + logo) del tenant actual para el sidebar/navbar del layout.
        public async Task<string> Branding()
        {
            var response = await _datosInstitucionService.ConsultarDatosInstitucion();
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        public async Task<string> Guardar(DatosInstitucion datos)
        {
            if (datos != null) datos.Id = TenantEmpresaId;
            var response = await _datosInstitucionService.GuardarDatosInstitucion(datos);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        public async Task<string> SubirLogo(HttpPostedFileBase logo)
        {
            var response = await _datosInstitucionService.GuardarLogo(logo);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<ActionResult> Logo()
        {
            var institucion = await _datosInstitucionService.ConsultarDatosInstitucion();
            if (institucion == null || !institucion.IsSuccess || institucion.Response == null ||
                string.IsNullOrWhiteSpace(institucion.Response.LogoRuta))
                return new HttpNotFoundResult();

            var bytes = await _datosInstitucionService.DescargarLogo();
            if (bytes == null) return new HttpNotFoundResult();

            return File(bytes, ObtenerContentType(institucion.Response.LogoRuta));
        }

        private string ObtenerContentType(string ruta)
        {
            var extension = Path.GetExtension(ruta ?? "").TrimStart('.').ToLowerInvariant();
            switch (extension)
            {
                case "png": return "image/png";
                case "jpg":
                case "jpeg": return "image/jpeg";
                default: return "application/octet-stream";
            }
        }
    }
}
