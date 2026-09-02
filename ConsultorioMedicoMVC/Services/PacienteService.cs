using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class PacienteService
    {
        private readonly HttpClientConnection _httpClient;

        public PacienteService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<PacienteDTO>>> ConsultarTodosPacientes()
            => await _httpClient.ObtenerPacientes();

        public async Task<Paciente> ObtenerPacientePorId(long id)
        {
            var r = await _httpClient.ObtenerPacientePorId(id);
            return (r.IsSuccess && r.Response != null) ? r.Response : null;
        }

        public async Task<ModelResponse<Paciente>> GuardarOActualizarPaciente(Paciente paciente)
            => await _httpClient.GuardarOActualizarPaciente(paciente);
    }
}
