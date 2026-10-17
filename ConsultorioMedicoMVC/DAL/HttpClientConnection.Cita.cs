using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<CitaDTO>>> ObtenerCitas()
            => await RequestAsync<List<CitaDTO>>("api/Cita/List", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Cita>> ObtenerCitaPorId(long id)
            => await RequestAsync<Cita>("api/Cita/" + id, HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Cita>> GuardarOActualizarCita(Cita cita)
            => await RequestAsync<Cita>("api/Cita/Guardar", HttpMethod.Post, cita, token.Token.access_token);

        public async Task<ModelResponse<bool>> EliminarCita(long id)
            => await RequestAsync<bool>("api/Cita/Eliminar/" + id, HttpMethod.Post, null, token.Token.access_token);

        public async Task<ModelResponse<List<CitaDTO>>> ObtenerCitasPorPaciente(long pacienteId)
            => await RequestAsync<List<CitaDTO>>("api/Cita/PorPaciente/" + pacienteId, HttpMethod.Get, null, token.Token.access_token);
    }
}
