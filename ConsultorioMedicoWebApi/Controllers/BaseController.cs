using System.Security.Claims;
using System.Web.Http;
using ConsultorioMedicoWebApi.DAL;

namespace ConsultorioMedicoWebApi.Controllers
{
    public class BaseController : ApiController
    {
        public DbWrapper dbWrapper;

        public BaseController()
        {
            dbWrapper = new DbWrapper();
        }

        public long ObtenerEmpresaIdDesdeClaim()
        {
            var identity = User.Identity as ClaimsIdentity;
            var claim = identity?.FindFirst("empresaId");
            long empresaId;
            if (claim != null && long.TryParse(claim.Value, out empresaId)) return empresaId;
            return 0;
        }
    }
}
