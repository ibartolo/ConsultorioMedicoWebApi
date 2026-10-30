using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class AnexoController : BaseController
    {
        private readonly AnexoService _anexoService;
        private readonly PacienteService _pacienteService;

        public AnexoController() : base()
        {
            _anexoService = new AnexoService(httpClientConnection);
            _pacienteService = new PacienteService(httpClientConnection);
        }

        #region Views
        public async Task<ActionResult> Index()
        {
            var pacientes = await _pacienteService.ConsultarTodosPacientes();
            ViewBag.Pacientes = (pacientes.IsSuccess && pacientes.Response != null) ? pacientes.Response : new List<PacienteDTO>();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Guardar(HttpPostedFileBase archivo, long pacienteId, long? consultaId, string comentarios)
        {
            var response = await _anexoService.GuardarAnexo(pacienteId, consultaId, comentarios, archivo);
            TempData[response.IsSuccess ? "Exito" : "Error"] = response.Message;
            return RedirectToAction("Index");
        }

        /// <summary>Subida AJAX desde el modal de anexos (paciente y opcionalmente consulta).</summary>
        [HttpPost]
        public async Task<string> SubirAnexo(HttpPostedFileBase archivo, long pacienteId, long? consultaId, string comentarios)
        {
            var response = await _anexoService.GuardarAnexo(pacienteId, consultaId, comentarios, archivo);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<ActionResult> Descargar(long id)
        {
            var anexo = await _anexoService.ObtenerAnexoPorId(id);
            var bytes = await _anexoService.DescargarBytes(id);
            if (bytes == null)
                return new HttpNotFoundResult();

            var nombre = anexo != null ? anexo.Nombre : "anexo";
            return File(bytes, ObtenerContentType(nombre), nombre);
        }

        [HttpPost]
        public async Task<string> Eliminar(long id)
        {
            var response = await _anexoService.EliminarAnexo(id);
            return JsonConvert.SerializeObject(response);
        }

        private string ObtenerContentType(string nombre)
        {
            var ext = System.IO.Path.GetExtension(nombre).TrimStart('.').ToLowerInvariant();
            switch (ext)
            {
                case "pdf": return "application/pdf";
                case "jpg":
                case "jpeg": return "image/jpeg";
                case "png": return "image/png";
                default: return "application/octet-stream";
            }
        }
        #endregion

        #region Data Access
        public async Task<string> ConsultarAnexos(long pacienteId)
        {
            var response = await _anexoService.ConsultarAnexosPorPaciente(pacienteId);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}
