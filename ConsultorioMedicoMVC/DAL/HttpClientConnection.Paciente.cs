using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<PacienteDTO>>> ObtenerPacientes()
            => await RequestAsync<List<PacienteDTO>>("api/Paciente/List", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Paciente>> ObtenerPacientePorId(long id)
            => await RequestAsync<Paciente>("api/Paciente/" + id, HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<Paciente>> GuardarOActualizarPaciente(Paciente paciente)
            => await RequestAsync<Paciente>("api/Paciente/Guardar", HttpMethod.Post, paciente, token.Token.access_token);
    }
}
