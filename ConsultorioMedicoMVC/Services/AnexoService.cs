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
    public class AnexoService
    {
        private readonly HttpClientConnection _httpClient;

        public AnexoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<Anexo>>> ConsultarAnexosPorPaciente(long pacienteId)
            => await _httpClient.ObtenerAnexosPorPaciente(pacienteId);

        public async Task<Anexo> ObtenerAnexoPorId(long id)
        {
            var r = await _httpClient.ObtenerAnexoPorId(id);
            return (r.IsSuccess && r.Response != null) ? r.Response : null;
        }

        public async Task<ModelResponse<Anexo>> GuardarAnexo(long pacienteId, long? consultaId, string comentarios, HttpPostedFileBase archivo)
        {
            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(pacienteId.ToString()), "pacienteId");
                if (consultaId.HasValue) form.Add(new StringContent(consultaId.Value.ToString()), "consultaId");
                form.Add(new StringContent(comentarios ?? ""), "comentarios");

                if (archivo != null && archivo.ContentLength > 0)
                {
                    byte[] bytes;
                    using (var ms = new MemoryStream())
                    {
                        archivo.InputStream.CopyTo(ms);
                        bytes = ms.ToArray();
                    }
                    var content = new ByteArrayContent(bytes);
                    content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(archivo.ContentType) ? "application/octet-stream" : archivo.ContentType);
                    form.Add(content, "archivos", archivo.FileName);
                }

                return await _httpClient.GuardarAnexo(form);
            }
        }

        public async Task<byte[]> DescargarBytes(long id)
            => await _httpClient.DescargarAnexo(id);
    }
}
