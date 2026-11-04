using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class TipoConsultaService
    {
        private readonly HttpClientConnection _httpClient;

        public TipoConsultaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<TipoConsulta>>> ConsultarTiposConsulta()
            => await _httpClient.ObtenerTiposConsulta();

        public async Task<ModelResponse<List<TipoConsulta>>> ConsultarTiposConsultaAdmin()
            => await _httpClient.ObtenerTiposConsultaAdmin();

        public async Task<TipoConsulta> ObtenerTipoConsultaPorId(long id)
        {
            var r = await _httpClient.ObtenerTipoConsultaPorId(id);
            return (r.IsSuccess && r.Response != null) ? r.Response : null;
        }

        public async Task<ModelResponse<TipoConsulta>> GuardarOActualizarTipoConsulta(TipoConsulta tipoConsulta)
        {
            // 1. Validación de negocio (mismas reglas que jQuery Validate)
            var error = TipoConsultaValidator.Validar(tipoConsulta);
            if (error != null)
                return new ModelResponse<TipoConsulta> { IsSuccess = false, Message = error };

            // 2. Lógica de alta
            if (tipoConsulta.Id == 0)
                tipoConsulta.Estatus = true;

            return await _httpClient.GuardarOActualizarTipoConsulta(tipoConsulta);
        }

        public async Task<ModelResponse<bool>> EliminarTipoConsulta(int id)
            => await _httpClient.EliminarTipoConsulta(id);
    }
}
