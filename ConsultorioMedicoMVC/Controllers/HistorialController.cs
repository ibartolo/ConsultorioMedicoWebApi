using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class HistorialController : BaseController
    {
        private readonly PacienteService _pacienteService;
        private readonly CitaService _citaService;
        private readonly ConsultaService _consultaService;
        private readonly AnexoService _anexoService;
        private readonly HistorialService _historialService;
        private readonly TipoConsultaService _tipoConsultaService;

        public HistorialController() : base()
        {
            _pacienteService = new PacienteService(httpClientConnection);
            _citaService = new CitaService(httpClientConnection);
            _consultaService = new ConsultaService(httpClientConnection);
            _anexoService = new AnexoService(httpClientConnection);
            _historialService = new HistorialService(httpClientConnection);
            _tipoConsultaService = new TipoConsultaService(httpClientConnection);
        }

        #region Views
        // GET: Historial (selección de paciente)
        public async Task<ActionResult> Index()
        {
            var pacientes = await _pacienteService.ConsultarTodosPacientes();
            ViewBag.Pacientes = (pacientes.IsSuccess && pacientes.Response != null) ? pacientes.Response : new List<PacienteDTO>();
            return View();
        }

        // GET: Historial/Expediente/{id} (expediente clínico del paciente)
        public async Task<ActionResult> Expediente(long id)
        {
            var paciente = await _pacienteService.ObtenerPacientePorId(id);
            if (paciente == null)
                return RedirectToAction("Index", "Paciente");

            var tratamientos = await _citaService.ConsultarTratamientos();
            ViewBag.Tratamientos = (tratamientos.IsSuccess && tratamientos.Response != null) ? tratamientos.Response : new List<Tratamiento>();

            var tipos = await _tipoConsultaService.ConsultarTiposConsulta();
            ViewBag.TiposConsulta = (tipos.IsSuccess && tipos.Response != null) ? tipos.Response : new List<TipoConsulta>();

            return View(paciente);
        }
        #endregion

        #region Data Access
        // AJAX: citas del paciente (DataTable)
        public async Task<string> ConsultarCitasPorPaciente(long pacienteId)
        {
            var response = await _citaService.ConsultarCitasPorPaciente(pacienteId);
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: consultas del paciente (DataTable)
        public async Task<string> ConsultarConsultasPorPaciente(long pacienteId)
        {
            var response = await _consultaService.ConsultarConsultasPorPaciente(pacienteId);
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: anexos del paciente (DataTable)
        public async Task<string> ConsultarAnexosPorPaciente(long pacienteId)
        {
            var response = await _anexoService.ConsultarAnexosPorPaciente(pacienteId);
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: tratamientos históricos del paciente (DataTable)
        public async Task<string> ConsultarTratamientosPorPaciente(long pacienteId)
        {
            var response = await _historialService.ConsultarTratamientosPorPaciente(pacienteId);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}
