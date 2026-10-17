using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<TratamientoHistoricoDTO>>> ObtenerTratamientosPorPaciente(long pacienteId)
            => await RequestAsync<List<TratamientoHistoricoDTO>>("api/Historial/TratamientosPorPaciente/" + pacienteId, HttpMethod.Get, null, token.Token.access_token);
    }
}
