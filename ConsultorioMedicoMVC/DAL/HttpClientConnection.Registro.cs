using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Registro;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        /// <summary>
        /// Envía el registro público a la WebApi. Endpoint anónimo: no requiere token
        /// (se pasa null para que HttpClientBase no agregue el encabezado Authorization).
        /// </summary>
        public async Task<ModelResponse<object>> GuardarRegistro(RegistroDTO datos)
            => await RequestAsync<object>("api/Registro/Guardar", HttpMethod.Post, datos, null);
    }
}
