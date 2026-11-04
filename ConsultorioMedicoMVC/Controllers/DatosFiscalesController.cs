using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class DatosFiscalesController : BaseController
    {
        private readonly DatosFiscalesService _datosFiscalesService;

        public DatosFiscalesController() : base()
        {
            _datosFiscalesService = new DatosFiscalesService(httpClientConnection);
        }

        /// <summary>Tenant resuelto desde la cookie de sesión (helper de BaseController). Sin fallback a 1.</summary>
        public ActionResult Index()
        {
            ViewBag.InstitucionId = TenantEmpresaId;
            return View();
        }

        public async Task<string> ConsultarTodos()
        {
            var response = await _datosFiscalesService.ConsultarDatosFiscales();
            return JsonConvert.SerializeObject(response);
        }

        // Devuelve la fila fiscal 1:1 del tenant actual (ya no "Unico" global).
        public async Task<string> Consultar()
        {
            var response = await _datosFiscalesService.ConsultarDatosFiscalActual();
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarPorId(int id)
        {
            var response = await _datosFiscalesService.ConsultarDatosFiscalPorId(id);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        public async Task<string> Guardar(DatosFiscales datos)
        {
            if (datos != null) datos.DatosInstitucionID = TenantEmpresaId;
            var response = await _datosFiscalesService.GuardarDatosFiscal(datos);
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        public async Task<string> Eliminar(int id)
        {
            var response = await _datosFiscalesService.EliminarDatosFiscal(id);
            return JsonConvert.SerializeObject(response);
        }
    }
}
