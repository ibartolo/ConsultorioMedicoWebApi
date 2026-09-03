using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<ConsultaDTO>>> ObtenerConsultas()
            => await RequestAsync<List<ConsultaDTO>>("api/Consulta/List", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Consulta>> ObtenerConsultaPorId(long id)
            => await RequestAsync<Consulta>("api/Consulta/" + id, HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Consulta>> GuardarOActualizarConsulta(Consulta consulta)
            => await RequestAsync<Consulta>("api/Consulta/Guardar", HttpMethod.Post, consulta, token.Token.access_token);

        public async Task<ModelResponse<RecetaDTO>> ObtenerRecetaPorConsulta(long consultaId)
            => await RequestAsync<RecetaDTO>("api/Consulta/Receta/" + consultaId, HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<List<ConsultaDTO>>> ObtenerConsultasPorPaciente(long pacienteId)
            => await RequestAsync<List<ConsultaDTO>>("api/Consulta/PorPaciente/" + pacienteId, HttpMethod.Get, null, token.Token.access_token);
    }
}
