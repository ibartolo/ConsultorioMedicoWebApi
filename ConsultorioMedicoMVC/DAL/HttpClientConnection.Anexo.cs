using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<Anexo>> GuardarAnexo(MultipartFormDataContent form)
            => await SendMultipartAsync<Anexo>("api/Anexo/Guardar", form, token.Token.access_token);

        public async Task<ModelResponse<List<Anexo>>> ObtenerAnexosPorPaciente(long pacienteId)
            => await RequestAsync<List<Anexo>>("api/Anexo/PorPaciente/" + pacienteId, HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Anexo>> ObtenerAnexoPorId(long id)
            => await RequestAsync<Anexo>("api/Anexo/" + id, HttpMethod.Get, null, token.Token.access_token);

        public async Task<byte[]> DescargarAnexo(long id)
            => await DescargarBytesAsync("api/Anexo/Descargar/" + id, token.Token.access_token);
    }
}
