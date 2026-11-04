using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<TipoConsulta>>> ObtenerTiposConsulta()
            => await RequestAsync<List<TipoConsulta>>("api/TipoConsulta/List", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<List<TipoConsulta>>> ObtenerTiposConsultaAdmin()
            => await RequestAsync<List<TipoConsulta>>("api/TipoConsulta/ListAdmin", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<TipoConsulta>> ObtenerTipoConsultaPorId(long id)
            => await RequestAsync<TipoConsulta>("api/TipoConsulta/" + id, HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<TipoConsulta>> GuardarOActualizarTipoConsulta(TipoConsulta tipoConsulta)
            => await RequestAsync<TipoConsulta>("api/TipoConsulta/Guardar", HttpMethod.Post, tipoConsulta, token.Token.access_token);

        public async Task<ModelResponse<bool>> EliminarTipoConsulta(int id)
            => await RequestAsync<bool>("api/TipoConsulta/Eliminar/" + id, HttpMethod.Post, null, token.Token.access_token);
    }
}
