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
            ViewBag.InstitucionId = (tokenCookie != null && tokenCookie.EmpresaID > 0)
                ? (int)tokenCookie.EmpresaID
                : 1;
            return View();
        }

        public async Task<string> Consultar(int id)
        {
            var response = await _datosInstitucionService.ConsultarDatosInstitucion(id);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        public async Task<string> Guardar(DatosInstitucion datos)
        {
            var response = await _datosInstitucionService.GuardarDatosInstitucion(datos);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        public async Task<string> SubirLogo(int id, HttpPostedFileBase logo)
        {
            var response = await _datosInstitucionService.GuardarLogo(id, logo);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<ActionResult> Logo(int id)
        {
            var institucion = await _datosInstitucionService.ConsultarDatosInstitucion(id);
            if (institucion == null || !institucion.IsSuccess || institucion.Response == null ||
                string.IsNullOrWhiteSpace(institucion.Response.LogoRuta))
                return new HttpNotFoundResult();

            var bytes = await _datosInstitucionService.DescargarLogo(id);
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
