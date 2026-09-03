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
    }
}
