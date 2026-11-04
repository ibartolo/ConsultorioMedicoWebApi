using System.Threading.Tasks;
using ConsultorioMedicoEntities.Registro;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class RegistroService
    {
        private readonly HttpClientConnection _httpClient;

        public RegistroService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<object>> GuardarRegistro(RegistroDTO datos)
        {
            var error = RegistroValidator.Validar(datos);
            if (error != null)
                return new ModelResponse<object> { IsSuccess = false, Message = error };

            return await _httpClient.GuardarRegistro(datos);
        }
    }
}
