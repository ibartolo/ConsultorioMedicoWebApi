using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class DatosInstitucionService
    {
        private readonly HttpClientConnection _httpClient;

        public DatosInstitucionService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<DatosInstitucion>> ConsultarDatosInstitucion(int id)
            => await _httpClient.ObtenerDatosInstitucion(id);

        public async Task<ModelResponse<DatosInstitucion>> GuardarDatosInstitucion(DatosInstitucion datos)
        {
            var error = DatosInstitucionValidator.Validar(datos);
            if (error != null)
                return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = error };

            return await _httpClient.GuardarDatosInstitucion(datos);
        }

        public async Task<ModelResponse<DatosInstitucion>> GuardarLogo(int id, HttpPostedFileBase archivo)
        {
            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(id.ToString()), "id");

                if (archivo != null && archivo.ContentLength > 0)
                {
                    byte[] bytes;
                    using (var ms = new MemoryStream())
                    {
                        archivo.InputStream.CopyTo(ms);
                        bytes = ms.ToArray();
                    }

                    var content = new ByteArrayContent(bytes);
                    content.Headers.ContentType = new MediaTypeHeaderValue(
                        string.IsNullOrWhiteSpace(archivo.ContentType) ? "application/octet-stream" : archivo.ContentType);
                    form.Add(content, "logo", archivo.FileName);
                }

                return await _httpClient.SubirLogo(form);
            }
        }

        public async Task<byte[]> DescargarLogo(int id)
            => await _httpClient.DescargarLogo(id);
    }
}
