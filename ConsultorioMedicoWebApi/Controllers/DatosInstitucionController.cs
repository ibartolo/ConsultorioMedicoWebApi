using System.IO;
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
    [RoutePrefix("api/DatosInstitucion")]
    public class DatosInstitucionController : BaseController
    {
        private readonly DatosInstitucionService _datosInstitucionService;

        public DatosInstitucionController()
        {
            _datosInstitucionService = new DatosInstitucionService();
        }

        // El Id de la institución ES el tenant: se resuelve siempre del claim (EmpresaId),
        // nunca del cliente. No se acepta id por ruta/query/body.
        [HttpGet, Route("")]
        public ModelResponse<DatosInstitucion> ObtenerDatosInstitucion()
            => _datosInstitucionService.ObtenerDatosInstitucion(EmpresaId, User.Identity.Name);

        [HttpPost, Route("Guardar")]
        public ModelResponse<DatosInstitucion> GuardarDatosInstitucion(DatosInstitucion datos)
            => _datosInstitucionService.GuardarDatosInstitucion(datos, EmpresaId, User.Identity.Name);

        [HttpPost, Route("SubirLogo")]
        public ModelResponse<DatosInstitucion> SubirLogo()
            => _datosInstitucionService.GuardarLogo(EmpresaId, HttpContext.Current.Request.Files, User.Identity.Name);

        [HttpGet, Route("Logo")]
        public HttpResponseMessage ObtenerLogo()
        {
            var institucion = _datosInstitucionService.ObtenerDatosInstitucion(EmpresaId, User.Identity.Name);
            if (institucion == null || !institucion.IsSuccess || institucion.Response == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Institución no encontrada.");

            var bytes = _datosInstitucionService.ObtenerBytesLogo(institucion.Response);
            if (bytes == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Logo no encontrado.");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(ObtenerContentType(institucion.Response.LogoRuta));
            return response;
        }

        private string ObtenerContentType(string ruta)
        {
            var extension = Path.GetExtension(ruta ?? "").TrimStart('.').ToLowerInvariant();
            switch (extension)
            {
                case "png": return "image/png";
                case "jpg":
                case "jpeg": return "image/jpeg";
                default: return "application/octet-stream";
            }
        }
    }
}
