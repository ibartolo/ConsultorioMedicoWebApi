using System.Threading.Tasks;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    /// <summary>
    /// Servicio del flujo de recuperación de contraseña. Solo reenvía a la WebApi;
    /// no agrega reglas de negocio (la validación de la nueva contraseña vive en la WebApi).
    /// </summary>
    public class RecuperacionPasswordService
    {
        private readonly HttpClientConnection _httpClient;

        public RecuperacionPasswordService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<object>> Solicitar(string email)
            => await _httpClient.SolicitarRecuperacion(email);

        public async Task<ModelResponse<object>> Validar(string token)
            => await _httpClient.ValidarTokenRecuperacion(token);

        public async Task<ModelResponse<object>> Restablecer(string token, string password, string confirmacion)
            => await _httpClient.RestablecerPassword(token, password, confirmacion);
    }
}
