using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class DashboardService
    {
        private readonly HttpClientConnection _httpClient;

        public DashboardService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<DashboardDTO>> ConsultarIndicadores()
            => await _httpClient.ObtenerIndicadoresDashboard();
    }
}
