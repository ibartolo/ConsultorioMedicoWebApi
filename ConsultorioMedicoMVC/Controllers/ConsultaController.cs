using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class ConsultaController : BaseController
    {
        private readonly ConsultaService _consultaService;
        private readonly PacienteService _pacienteService;
        private readonly TratamientoService _tratamientoService;
        private readonly TipoConsultaService _tipoConsultaService;

        public ConsultaController() : base()
        {
            _consultaService = new ConsultaService(httpClientConnection);
            _pacienteService = new PacienteService(httpClientConnection);
            _tratamientoService = new TratamientoService(httpClientConnection);
            _tipoConsultaService = new TipoConsultaService(httpClientConnection);
        }

        #region Views
        // GET: Consulta (calendario)
        public async Task<ActionResult> Index()
        {
            var pacientes = await _pacienteService.ConsultarTodosPacientes();
            ViewBag.Pacientes = (pacientes.IsSuccess && pacientes.Response != null) ? pacientes.Response : new List<PacienteDTO>();

            var tratamientos = await _tratamientoService.ConsultarTratamientos();
            ViewBag.Tratamientos = (tratamientos.IsSuccess && tratamientos.Response != null) ? tratamientos.Response : new List<Tratamiento>();

            var tipos = await _tipoConsultaService.ConsultarTiposConsulta();
            ViewBag.TiposConsulta = (tipos.IsSuccess && tipos.Response != null) ? tipos.Response : new List<TipoConsulta>();

            return View();
        }

        // GET: Consulta/Nuevo (alta) y Consulta/Nuevo/{id} (edición)
        public async Task<ActionResult> Nuevo(long id = 0)
        {
            var pacientes = await _pacienteService.ConsultarTodosPacientes();
            ViewBag.Pacientes = (pacientes.IsSuccess && pacientes.Response != null) ? pacientes.Response : new List<PacienteDTO>();

            var tratamientos = await _tratamientoService.ConsultarTratamientos();
            ViewBag.Tratamientos = (tratamientos.IsSuccess && tratamientos.Response != null) ? tratamientos.Response : new List<Tratamiento>();

            var tipos = await _tipoConsultaService.ConsultarTiposConsulta();
            ViewBag.TiposConsulta = (tipos.IsSuccess && tipos.Response != null) ? tipos.Response : new List<TipoConsulta>();

            if (id > 0)
            {
                var consulta = await _consultaService.ObtenerConsultaPorId(id);
                return View(consulta ?? new Consulta());
            }
            return View(new Consulta());
        }

        // GET: Consulta/Receta/{id} (receta imprimible)
        public async Task<ActionResult> Receta(long id)
        {
            var institucionId = (tokenCookie != null && tokenCookie.EmpresaID > 0)
                ? (int)tokenCookie.EmpresaID
                : 1;

            var receta = await _consultaService.ObtenerRecetaPorConsulta(id, institucionId);
            if (receta == null || !receta.IsSuccess || receta.Response == null)
                return RedirectToAction("Index");

            ViewBag.InstitucionId = institucionId;
            return View(receta.Response);
        }

        // GET: Consulta/RecetaParcial/{id} (contenido para el modal)
        public async Task<ActionResult> RecetaParcial(long id)
        {
            var institucionId = (tokenCookie != null && tokenCookie.EmpresaID > 0)
                ? (int)tokenCookie.EmpresaID
                : 1;

            var receta = await _consultaService.ObtenerRecetaPorConsulta(id, institucionId);
            if (receta == null || !receta.IsSuccess || receta.Response == null)
                return new HttpNotFoundResult();

            ViewBag.InstitucionId = institucionId;
            return PartialView("_RecetaContenido", receta.Response);
        }
        #endregion

        #region Data Access
        // AJAX: listado de consultas (DataTable)
        public async Task<string> ConsultarTodasConsultas()
        {
            var response = await _consultaService.ConsultarTodasConsultas();
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: guardar o actualizar (recibe la entidad Consulta por model binding)
        [HttpPost]
        public async Task<string> GuardarOActualizarConsulta(Consulta consulta)
        {
            var response = await _consultaService.GuardarOActualizarConsulta(consulta);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}
