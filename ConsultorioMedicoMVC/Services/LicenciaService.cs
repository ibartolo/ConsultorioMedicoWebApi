using System.Collections.Generic;
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
    public class LicenciaService
    {
        private readonly HttpClientConnection _httpClient;

        public LicenciaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<LicenciaDTO>> ObtenerLicencia()
            => await _httpClient.ObtenerLicencia();

        public async Task<ModelResponse<List<ConsultorioPago>>> ObtenerPagos()
            => await _httpClient.ObtenerPagos();

        public async Task<ModelResponse<ConsultorioPago>> SubirComprobante(HttpPostedFileBase archivo)
        {
            using (var form = new MultipartFormDataContent())
            {
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
                    form.Add(content, "archivos", archivo.FileName);
                }

                return await _httpClient.SubirComprobante(form);
            }
        }

        public async Task<byte[]> DescargarComprobante(int pagoId)
            => await _httpClient.DescargarComprobante(pagoId);
    }
}
