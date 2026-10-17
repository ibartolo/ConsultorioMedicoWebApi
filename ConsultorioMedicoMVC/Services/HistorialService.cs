using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class HistorialService
    {
        private readonly HttpClientConnection _httpClient;

        public HistorialService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<TratamientoHistoricoDTO>>> ConsultarTratamientosPorPaciente(long pacienteId)
            => await _httpClient.ObtenerTratamientosPorPaciente(pacienteId);
    }
}
