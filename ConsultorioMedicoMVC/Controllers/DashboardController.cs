using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly DashboardService _dashboardService;

        public DashboardController() : base()
        {
            _dashboardService = new DashboardService(httpClientConnection);
        }

        public async Task<string> Indicadores()
        {
            var response = await _dashboardService.ConsultarIndicadores();
            return JsonConvert.SerializeObject(response);
        }
    }
}
