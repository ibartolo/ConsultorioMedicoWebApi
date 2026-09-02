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

        // AJAX: guardar o actualizar (doble encode JSON)
        [HttpPost]
        public async Task<string> GuardarOActualizarPaciente(Paciente paciente)
        {
            try
            {
                if (paciente.Id == 0)
                {
                    paciente.Estatus = true;
                    paciente.FechaAlta = DateTime.Now;
                }
                else
                {
                    var existente = await _pacienteService.ObtenerPacientePorId(paciente.Id);
                    if (existente != null) paciente.FechaAlta = existente.FechaAlta;
                }

                var response = await _pacienteService.GuardarOActualizarPaciente(paciente);
                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GuardarOActualizarPaciente error");
                return JsonConvert.SerializeObject(new ConsultorioMedicoEntities.Seguridad.ModelResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }
        #endregion
    }
}
