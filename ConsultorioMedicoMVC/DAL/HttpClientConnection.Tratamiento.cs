using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<Tratamiento>>> ObtenerTratamientos()
            => await RequestAsync<List<Tratamiento>>("api/Tratamiento/List", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Tratamiento>> ObtenerTratamientoPorId(long id)
            => await RequestAsync<Tratamiento>("api/Tratamiento/" + id, HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Tratamiento>> GuardarOActualizarTratamiento(Tratamiento tratamiento)
            => await RequestAsync<Tratamiento>("api/Tratamiento/Guardar", HttpMethod.Post, tratamiento, token.Token.access_token);
    }
}
