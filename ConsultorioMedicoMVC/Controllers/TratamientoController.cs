using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class TratamientoController : BaseController
    {
        private readonly TratamientoService _tratamientoService;

        public TratamientoController() : base()
        {
            _tratamientoService = new TratamientoService(httpClientConnection);
        }

        #region Views
        // GET: Tratamiento (catálogo / listado)
        public ActionResult Index()
        {
            return View();
        }

        // GET: Tratamiento/Nuevo (alta) y Tratamiento/Nuevo/{id} (edición)
        public async Task<ActionResult> Nuevo(long id = 0)
        {
            if (id > 0)
            {
                var tratamiento = await _tratamientoService.ObtenerTratamientoPorId(id);
                return View(tratamiento ?? new Tratamiento());
            }
            return View(new Tratamiento());
        }
        #endregion

        #region Data Access
        // AJAX: listado de tratamientos (DataTable)
        public async Task<string> ConsultarTodosTratamientos()
        {
            var response = await _tratamientoService.ConsultarTratamientos();
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: guardar o actualizar (recibe la entidad Tratamiento por model binding)
        [HttpPost]
        public async Task<string> GuardarOActualizarTratamiento(Tratamiento tratamiento)
        {
            var response = await _tratamientoService.GuardarOActualizarTratamiento(tratamiento);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}
