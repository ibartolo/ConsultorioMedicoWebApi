using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class PaqueteController : BaseController
    {
        private readonly PaqueteService _paqueteService;

        public PaqueteController() : base()
        {
            _paqueteService = new PaqueteService(httpClientConnection);
        }

        #region Views
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Nuevo(long id = 0)
        {
            if (id > 0)
            {
                var paquete = await _paqueteService.ObtenerPaquetePorId(id);
                return View(paquete ?? new Paquete());
            }
            return View(new Paquete());
        }
        #endregion

        #region Data Access
        public async Task<string> ConsultarTodosPaquetes()
        {
            var response = await _paqueteService.ConsultarTodosPaquetes();
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost]
        public async Task<string> GuardarOActualizarPaquete(Paquete paquete)
        {
            var response = await _paqueteService.GuardarOActualizarPaquete(paquete);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}
