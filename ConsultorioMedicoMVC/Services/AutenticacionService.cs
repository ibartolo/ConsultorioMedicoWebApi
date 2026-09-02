using System.Threading.Tasks;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class AutenticacionService
    {
        private readonly HttpClientConnection _httpClient;

        public AutenticacionService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<UsuarioDTO>> AutenticarUsuario(Usuario usuario)
            => await _httpClient.AutenticarUsuario(usuario);
    }
}
