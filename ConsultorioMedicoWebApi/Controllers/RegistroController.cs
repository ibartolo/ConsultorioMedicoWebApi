using System.Web.Http;
using ConsultorioMedicoEntities.Registro;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.Services;

namespace ConsultorioMedicoWebApi.Controllers
{
    /// <summary>
    /// Registro público de consultorio. Endpoint anónimo (sin token OAuth).
    /// </summary>
    [AllowAnonymous]
    [RoutePrefix("api/Registro")]
    public class RegistroController : BaseController
    {
        private readonly RegistroService _registroService;

        public RegistroController()
        {
            _registroService = new RegistroService();
        }

        [HttpPost, Route("Guardar")]
        public ModelResponse Guardar(RegistroDTO datos)
            => _registroService.RegistrarConsultorio(datos);
    }
}
