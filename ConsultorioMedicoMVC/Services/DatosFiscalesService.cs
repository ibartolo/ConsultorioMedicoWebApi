using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class DatosFiscalesService
    {
        private readonly HttpClientConnection _httpClient;

        public DatosFiscalesService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<DatosFiscales>>> ConsultarDatosFiscales()
            => await _httpClient.ObtenerDatosFiscales();

        public async Task<ModelResponse<DatosFiscales>> ConsultarDatosFiscalUnico()
            => await _httpClient.ObtenerDatosFiscalUnico();

        public async Task<ModelResponse<DatosFiscales>> ConsultarDatosFiscalPorId(int id)
            => await _httpClient.ObtenerDatosFiscalPorId(id);

        public async Task<ModelResponse<DatosFiscales>> GuardarDatosFiscal(DatosFiscales datos)
        {
            var error = DatosFiscalesValidator.Validar(datos);
            if (error != null)
                return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = error };

            return await _httpClient.GuardarDatosFiscal(datos);
        }

        public async Task<ModelResponse<bool>> EliminarDatosFiscal(int id)
            => await _httpClient.EliminarDatosFiscal(id);
    }
}
