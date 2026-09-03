using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class PaqueteService
    {
        private readonly HttpClientConnection _httpClient;

        public PaqueteService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<Paquete>>> ConsultarTodosPaquetes()
            => await _httpClient.ObtenerPaquetes();

        public async Task<Paquete> ObtenerPaquetePorId(long id)
        {
            var r = await _httpClient.ObtenerPaquetePorId(id);
            return (r.IsSuccess && r.Response != null) ? r.Response : null;
        }

        public async Task<ModelResponse<Paquete>> GuardarOActualizarPaquete(Paquete paquete)
        {
            var error = PaqueteValidator.Validar(paquete);
            if (error != null)
                return new ModelResponse<Paquete> { IsSuccess = false, Message = error };

            if (paquete.Id == 0)
            {
                paquete.Estatus = true;
                paquete.FechaCreacion = System.DateTime.Now;
            }

            return await _httpClient.GuardarOActualizarPaquete(paquete);
        }
    }
}
