using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<DatosFiscales>>> ObtenerDatosFiscales()
            => await RequestAsync<List<DatosFiscales>>("api/DatosFiscales/List", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<DatosFiscales>> ObtenerDatosFiscalUnico()
            => await RequestAsync<DatosFiscales>("api/DatosFiscales/Unico", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<DatosFiscales>> ObtenerDatosFiscalPorId(int id)
            => await RequestAsync<DatosFiscales>("api/DatosFiscales/" + id, HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<DatosFiscales>> GuardarDatosFiscal(DatosFiscales datos)
            => await RequestAsync<DatosFiscales>("api/DatosFiscales/Guardar", HttpMethod.Post, datos, token.Token.access_token);

        public async Task<ModelResponse<bool>> EliminarDatosFiscal(int id)
            => await RequestAsync<bool>("api/DatosFiscales/Eliminar/" + id, HttpMethod.Post, null, token.Token.access_token);
    }
}
