using System.Web.Http;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Dashboard")]
    public class DashboardController : BaseController
    {
        private readonly DashboardService _dashboardService;

        public DashboardController()
        {
            _dashboardService = new DashboardService();
        }

        [HttpGet, Route("Indicadores")]
        public ModelResponse<DashboardDTO> ObtenerIndicadores()
            => _dashboardService.ObtenerIndicadores(User.Identity.Name);
    }
}
