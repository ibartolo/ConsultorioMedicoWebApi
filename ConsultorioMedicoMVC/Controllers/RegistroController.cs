using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Registro;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    /// <summary>
    /// Registro público de consultorio. Sin [Authorize]; el filtro lo permite vía PublicActions.
    /// </summary>
    public class RegistroController : BaseController
    {
        private readonly RegistroService _registroService;

        public RegistroController() : base()
        {
            _registroService = new RegistroService(httpClientConnection);
        }

        // GET: Registro/Index (público)
        public ActionResult Index()
        {
            return View();
        }

        // POST: Registro/Guardar (público) → devuelve JSON para la vista.
        [HttpPost]
        public async Task<string> Guardar(RegistroDTO datos)
        {
            var response = await _registroService.GuardarRegistro(datos);
            return JsonConvert.SerializeObject(response);
        }
    }
}
