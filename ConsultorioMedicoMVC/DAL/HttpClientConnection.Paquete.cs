using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<Paquete>>> ObtenerPaquetes()
            => await RequestAsync<List<Paquete>>("api/Paquete/List", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Paquete>> ObtenerPaquetePorId(long id)
            => await RequestAsync<Paquete>("api/Paquete/" + id, HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Paquete>> GuardarOActualizarPaquete(Paquete paquete)
            => await RequestAsync<Paquete>("api/Paquete/Guardar", HttpMethod.Post, paquete, token.Token.access_token);
    }
}
