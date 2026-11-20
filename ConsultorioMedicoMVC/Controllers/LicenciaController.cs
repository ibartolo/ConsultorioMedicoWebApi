using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class LicenciaController : BaseController
    {
        private readonly LicenciaService _licenciaService;

        public LicenciaController() : base()
        {
            _licenciaService = new LicenciaService(httpClientConnection);
        }

        #region Views
        public ActionResult Index()
        {
            ViewBag.PagoBanco = ConfigurationManager.AppSettings["PagoBanco"];
            ViewBag.PagoClabe = ConfigurationManager.AppSettings["PagoClabe"];
            ViewBag.PagoTitular = ConfigurationManager.AppSettings["PagoTitular"];
            ViewBag.PagoMontoAnual = ConfigurationManager.AppSettings["PagoMontoAnual"];
            ViewBag.VigenciaVencida = tokenCookie != null && tokenCookie.VigenciaVencida;
            return View();
        }
        #endregion

        #region Data Access
        public async Task<string> Consultar()
        {
            var response = await _licenciaService.ObtenerLicencia();
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarPagos()
        {
            var response = await _licenciaService.ObtenerPagos();
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        public async Task<string> SubirComprobante(HttpPostedFileBase archivo)
        {
            var response = await _licenciaService.SubirComprobante(archivo);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<ActionResult> Descargar(int id)
        {
            var bytes = await _licenciaService.DescargarComprobante(id);
            if (bytes == null)
                return new HttpNotFoundResult();

            // Nombre para la descarga (best-effort desde el historial del tenant).
            var nombre = "comprobante";
            var pagos = await _licenciaService.ObtenerPagos();
            if (pagos != null && pagos.IsSuccess && pagos.Response != null)
            {
                var pago = pagos.Response.FirstOrDefault(p => p.Id == id);
                if (pago != null && !string.IsNullOrWhiteSpace(pago.NombreArchivo))
                    nombre = pago.NombreArchivo;
            }

            return File(bytes, ObtenerContentType(nombre), nombre);
        }

        private string ObtenerContentType(string nombre)
        {
            var ext = System.IO.Path.GetExtension(nombre ?? "").TrimStart('.').ToLowerInvariant();
            switch (ext)
            {
                case "pdf": return "application/pdf";
                case "jpg":
                case "jpeg": return "image/jpeg";
                case "png": return "image/png";
                default: return "application/octet-stream";
            }
        }
        #endregion
    }
}
