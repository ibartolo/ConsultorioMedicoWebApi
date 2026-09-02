using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<UsuarioDTO>> AutenticarUsuario(Usuario usuario)
            => await RequestAsync<UsuarioDTO>("api/Autenticacion/autenticar", HttpMethod.Post, usuario, null);
    }
}
