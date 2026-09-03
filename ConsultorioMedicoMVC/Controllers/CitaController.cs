using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class CitaController : BaseController
    {
        private readonly CitaService _citaService;
        private readonly PacienteService _pacienteService;

        public CitaController() : base()
        {
            _citaService = new CitaService(httpClientConnection);
            _pacienteService = new PacienteService(httpClientConnection);
        }

        #region Views
        // GET: Cita (calendario)
        public async Task<ActionResult> Index()
        {
            var pacientes = await _pacienteService.ConsultarTodosPacientes();
            ViewBag.Pacientes = (pacientes.IsSuccess && pacientes.Response != null) ? pacientes.Response : new List<PacienteDTO>();

            var tratamientos = await _citaService.ConsultarTratamientos();
            ViewBag.Tratamientos = (tratamientos.IsSuccess && tratamientos.Response != null) ? tratamientos.Response : new List<Tratamiento>();

            return View();
        }

        // GET: Cita/Nuevo (alta) y Cita/Nuevo/{id} (edición)
        public async Task<ActionResult> Nuevo(long id = 0)
        {
            // Dropdown de pacientes
            var pacientes = await _pacienteService.ConsultarTodosPacientes();
            ViewBag.Pacientes = (pacientes.IsSuccess && pacientes.Response != null)
                ? pacientes.Response
                : new List<PacienteDTO>();

            // Dropdown de tratamientos
            var tratamientos = await _citaService.ConsultarTratamientos();
            ViewBag.Tratamientos = (tratamientos.IsSuccess && tratamientos.Response != null)
                ? tratamientos.Response
                : new List<Tratamiento>();

            if (id > 0)
            {
                var cita = await _citaService.ObtenerCitaPorId(id);
                return View(cita ?? new Cita());
            }
            return View(new Cita());
        }
        #endregion

        #region Data Access
        // AJAX: listado de citas (DataTable)
        public async Task<string> ConsultarTodasCitas()
        {
            var response = await _citaService.ConsultarTodasCitas();
            return JsonConvert.SerializeObject(response);
        }

        // AJAX: guardar o actualizar (recibe la entidad Cita por model binding)
        [HttpPost]
        public async Task<string> GuardarOActualizarCita(Cita cita)
        {
            var response = await _citaService.GuardarOActualizarCita(cita);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}
