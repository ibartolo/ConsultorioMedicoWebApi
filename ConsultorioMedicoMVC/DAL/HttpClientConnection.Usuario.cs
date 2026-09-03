using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<Usuario>>> ObtenerUsuarios()
            => await RequestAsync<List<Usuario>>("api/Usuario/List", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Usuario>> ObtenerUsuarioPorId(long id)
            => await RequestAsync<Usuario>("api/Usuario/" + id, HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Usuario>> GuardarOActualizarUsuario(Usuario usuario)
            => await RequestAsync<Usuario>("api/Usuario/Guardar", HttpMethod.Post, usuario, token.Token.access_token);
    }
}
