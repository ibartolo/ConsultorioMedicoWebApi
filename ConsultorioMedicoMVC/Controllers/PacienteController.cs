using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;
using Serilog;

namespace ConsultorioMedicoMVC.Controllers
{
    public class PacienteController : BaseController
    {
        private readonly PacienteService _pacienteService;

        public PacienteController() : base()
        {
            _pacienteService = new PacienteService(httpClientConnection);
        }

        #region Views
        // GET: Paciente (catálogo / listado)
        public ActionResult Index()
        {
            return View();
        }

        // GET: Paciente/Nuevo (alta) y Paciente/Nuevo?id=N (edición)
        public async Task<ActionResult> Nuevo(long id = 0)
        {
            if (id > 0)
            {
                var paciente = await _pacienteService.ObtenerPacientePorId(id);
                return View(paciente ?? new Paciente());
            }
            return View(new Paciente());
        }
        #endregion

        #region Data Access
        // AJAX: listado de pacientes (DataTable)
        public async Task<string> ConsultarTodosPacientes()
        {
            var response = await _pacienteService.ConsultarTodosPacientes();
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: guardar o actualizar (recibe la entidad Paciente por model binding)
        [HttpPost]
        public async Task<string> GuardarOActualizarPaciente(Paciente paciente)
        {
            var response = await _pacienteService.GuardarOActualizarPaciente(paciente);
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: consultar paciente por Id para poblar el modal (devuelve PacienteDTO con NombreCompleto)
        public async Task<string> ConsultarPacientePorId(long id)
        {
            var response = await _pacienteService.ConsultarPacientePorId(id);
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: baja lógica de paciente (Estatus=0)
        [HttpPost]
        public async Task<string> EliminarPaciente(long id)
        {
            var response = await _pacienteService.EliminarPaciente(id);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}
