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
    [RoutePrefix("api/Licencia")]
    public class LicenciaController : BaseController
    {
        private readonly LicenciaService _licenciaService;

        public LicenciaController()
        {
            _licenciaService = new LicenciaService();
        }

        [HttpGet, Route("")]
        public ModelResponse<LicenciaDTO> ObtenerLicencia()
            => _licenciaService.ObtenerLicencia(EmpresaId, User.Identity.Name);

        [HttpGet, Route("Pagos")]
        public ModelResponse<List<ConsultorioPago>> Pagos()
            => _licenciaService.ObtenerPagosLicencia(EmpresaId, User.Identity.Name);

        [HttpPost, Route("SubirComprobante")]
        public ModelResponse<ConsultorioPago> SubirComprobante()
            => _licenciaService.GuardarPagoLicencia(EmpresaId, User.Identity.Name, HttpContext.Current.Request.Files);

        // Descarga por proxy; el pago se resuelve SIEMPRE por tenant (claim).
        // sp_nh_ConfirmarPagoLicencia NO se expone (confirmación manual).
        [HttpGet, Route("Descargar/{pagoId:int}")]
        public HttpResponseMessage Descargar(int pagoId)
        {
            var pago = _licenciaService.ObtenerPagoLicenciaById(pagoId, EmpresaId, User.Identity.Name);
            if (pago == null || !pago.IsSuccess || pago.Response == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Comprobante no encontrado.");

            var bytes = _licenciaService.ObtenerBytesPago(pago.Response);
            if (bytes == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Archivo no encontrado.");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(ObtenerContentType(pago.Response.NombreArchivo));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = pago.Response.NombreArchivo
            };
            return response;
        }

        private string ObtenerContentType(string nombreArchivo)
        {
            var extension = System.IO.Path.GetExtension(nombreArchivo ?? "").TrimStart('.').ToLowerInvariant();
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
