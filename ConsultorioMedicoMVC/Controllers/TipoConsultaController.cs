using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class TipoConsultaController : BaseController
    {
        private readonly TipoConsultaService _tipoConsultaService;

        public TipoConsultaController() : base()
        {
            _tipoConsultaService = new TipoConsultaService(httpClientConnection);
        }

        #region Views
        // GET: TipoConsulta (catálogo / listado)
        public ActionResult Index()
        {
            return View();
        }

        // GET: TipoConsulta/Nuevo (alta) y TipoConsulta/Nuevo/{id} (edición)
        public async Task<ActionResult> Nuevo(long id = 0)
        {
            if (id > 0)
            {
                var tipo = await _tipoConsultaService.ObtenerTipoConsultaPorId(id);
                return View(tipo ?? new TipoConsulta());
            }
            return View(new TipoConsulta());
        }
        #endregion

        #region Data Access
        // AJAX: listado de tipos de consulta (DataTable). Usa ListAdmin para mostrar también los inactivos
        // (con badge de Estatus) y permitir edición/soft-delete desde la pantalla de administración.
        public async Task<string> ConsultarTodosTiposConsulta()
        {
            var response = await _tipoConsultaService.ConsultarTiposConsultaAdmin();
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: guardar o actualizar (recibe la entidad TipoConsulta por model binding)
        [HttpPost]
        public async Task<string> GuardarOActualizarTipoConsulta(TipoConsulta tipoConsulta)
        {
            var response = await _tipoConsultaService.GuardarOActualizarTipoConsulta(tipoConsulta);
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: desactivar (baja lógica / soft delete)
        [HttpPost]
        public async Task<string> EliminarTipoConsulta(int id)
        {
            var response = await _tipoConsultaService.EliminarTipoConsulta(id);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}
