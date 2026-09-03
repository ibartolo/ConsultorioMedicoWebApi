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

        public HistorialController() : base()
        {
            _pacienteService = new PacienteService(httpClientConnection);
            _citaService = new CitaService(httpClientConnection);
            _consultaService = new ConsultaService(httpClientConnection);
            _anexoService = new AnexoService(httpClientConnection);
        }

        public async Task<ActionResult> Index()
        {
            var pacientes = await _pacienteService.ConsultarTodosPacientes();
            ViewBag.Pacientes = (pacientes.IsSuccess && pacientes.Response != null) ? pacientes.Response : new List<PacienteDTO>();
            return View();
        }

        public async Task<string> ConsultarCitasPorPaciente(long pacienteId)
        {
            var response = await _citaService.ConsultarCitasPorPaciente(pacienteId);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarConsultasPorPaciente(long pacienteId)
        {
            var response = await _consultaService.ConsultarConsultasPorPaciente(pacienteId);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ConsultarAnexosPorPaciente(long pacienteId)
        {
            var response = await _anexoService.ConsultarAnexosPorPaciente(pacienteId);
            return JsonConvert.SerializeObject(response);
        }
    }
}
