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

        /// <summary>
        /// Tenant (id de consultorio) tipado como int, derivado exclusivamente del claim
        /// `empresaId`. Devuelve 0 si el claim no existe. Nunca se toma del cliente.
        /// </summary>
        public int EmpresaId
        {
            get { return (int)ObtenerEmpresaIdDesdeClaim(); }
        }
    }
}
