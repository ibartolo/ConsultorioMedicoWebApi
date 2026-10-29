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

        public ActionResult Index()
        {
            ViewBag.InstitucionId = (tokenCookie != null && tokenCookie.EmpresaID > 0)
                ? (int)tokenCookie.EmpresaID
                : 1;
            return View();
        }

        public async Task<string> ConsultarTodos()
        {
            var response = await _datosFiscalesService.ConsultarDatosFiscales();
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> Consultar()
        {
            var response = await _datosFiscalesService.ConsultarDatosFiscalUnico();
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
