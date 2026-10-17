using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class CitaService
    {
        private readonly HttpClientConnection _httpClient;

        public CitaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<CitaDTO>>> ConsultarTodasCitas()
            => await _httpClient.ObtenerCitas();

        public async Task<Cita> ObtenerCitaPorId(long id)
        {
            var r = await _httpClient.ObtenerCitaPorId(id);
            return (r.IsSuccess && r.Response != null) ? r.Response : null;
        }

        public async Task<ModelResponse<List<Tratamiento>>> ConsultarTratamientos()
            => await _httpClient.ObtenerTratamientos();

        public async Task<ModelResponse<Cita>> GuardarOActualizarCita(Cita cita)
        {
            // 1. Validación de negocio (mismas reglas que jQuery Validate)
            var error = CitaValidator.Validar(cita);
            if (error != null)
                return new ModelResponse<Cita> { IsSuccess = false, Message = error };

            // 2. Lógica de alta
            if (cita.Id == 0)
                cita.Estatus = true;

            return await _httpClient.GuardarOActualizarCita(cita);
        }

        public async Task<ModelResponse<bool>> EliminarCita(long id)
            => await _httpClient.EliminarCita(id);

        public async Task<ModelResponse<List<CitaDTO>>> ConsultarCitasPorPaciente(long pacienteId)
            => await _httpClient.ObtenerCitasPorPaciente(pacienteId);
    }
}
