using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Anexo")]
    public class AnexoController : BaseController
    {
        private readonly AnexoService _anexoService;

        public AnexoController()
        {
            _anexoService = new AnexoService();
        }

        [HttpPost, Route("Guardar")]
        public ModelResponse<Anexo> Guardar()
        {
            var usuario = User.Identity.Name;

            long pacienteId;
            if (!long.TryParse(HttpContext.Current.Request.Form["pacienteId"], out pacienteId))
                return new ModelResponse<Anexo> { IsSuccess = false, Message = "PacienteId requerido." };

            long? consultaId = null;
            long cid;
            if (long.TryParse(HttpContext.Current.Request.Form["consultaId"], out cid) && cid > 0)
                consultaId = cid;

            var comentarios = HttpContext.Current.Request.Form["comentarios"];

            return _anexoService.GuardarAnexo(pacienteId, consultaId, comentarios, HttpContext.Current.Request.Files, usuario);
        }

        [HttpGet, Route("PorPaciente/{pacienteId:long}")]
        public ModelResponse<List<Anexo>> PorPaciente(long pacienteId)
            => _anexoService.ObtenerAnexosPorPaciente(pacienteId, User.Identity.Name);

        [HttpGet, Route("{id:long}")]
        public ModelResponse<Anexo> ObtenerAnexoPorId(long id)
            => _anexoService.ObtenerAnexoPorId(id, User.Identity.Name);

        [HttpGet, Route("Descargar/{id:long}")]
        public HttpResponseMessage Descargar(long id)
        {
            var anexo = _anexoService.ObtenerAnexoPorId(id, User.Identity.Name);
            if (anexo == null || !anexo.IsSuccess || anexo.Response == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Anexo no encontrado.");

            var bytes = _anexoService.ObtenerBytesAnexo(anexo.Response);
            if (bytes == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Archivo no encontrado.");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(ObtenerContentType(anexo.Response.Nombre));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = anexo.Response.Nombre
            };
            return response;
        }

        private string ObtenerContentType(string nombreArchivo)
        {
            var extension = System.IO.Path.GetExtension(nombreArchivo).TrimStart('.').ToLowerInvariant();
            switch (extension)
            {
                case "pdf": return "application/pdf";
                case "jpg":
                case "jpeg": return "image/jpeg";
                case "png": return "image/png";
                default: return "application/octet-stream";
            }
        }
    }
}
