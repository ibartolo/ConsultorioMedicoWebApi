using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class ConsultaService
    {
        private readonly HttpClientConnection _httpClient;

        public ConsultaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<ConsultaDTO>>> ConsultarTodasConsultas()
            => await _httpClient.ObtenerConsultas();

        public async Task<Consulta> ObtenerConsultaPorId(long id)
        {
            var r = await _httpClient.ObtenerConsultaPorId(id);
            return (r.IsSuccess && r.Response != null) ? r.Response : null;
        }

        public async Task<ModelResponse<Consulta>> GuardarOActualizarConsulta(Consulta consulta)
        {
            // 1. Validación de negocio (mismas reglas que jQuery Validate)
            var error = ConsultaValidator.Validar(consulta);
            if (error != null)
                return new ModelResponse<Consulta> { IsSuccess = false, Message = error };

            // 2. Lógica de alta
            if (consulta.Id == 0)
                consulta.Estatus = true;

            return await _httpClient.GuardarOActualizarConsulta(consulta);
        }

        public async Task<ModelResponse<RecetaDTO>> ObtenerRecetaPorConsulta(long consultaId)
            => await _httpClient.ObtenerRecetaPorConsulta(consultaId);

        public async Task<ModelResponse<List<ConsultaDTO>>> ConsultarConsultasPorPaciente(long pacienteId)
            => await _httpClient.ObtenerConsultasPorPaciente(pacienteId);
    }
}
