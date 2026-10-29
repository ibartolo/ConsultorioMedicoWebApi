using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<DashboardDTO>> ObtenerIndicadoresDashboard()
            => await RequestAsync<DashboardDTO>("api/Dashboard/Indicadores", HttpMethod.Get, null, token.Token.access_token);
    }
}
