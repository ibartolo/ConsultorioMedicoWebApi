using System;
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
        {
            // 1. Validación de negocio (mismas reglas que jQuery Validate)
            var error = PacienteValidator.Validar(paciente);
            if (error != null)
                return new ModelResponse<Paciente> { IsSuccess = false, Message = error };

            // 2. Lógica de alta (auditoría por defecto)
            if (paciente.Id == 0)
            {
                paciente.Estatus = true;
                paciente.FechaAlta = DateTime.Now;
            }

            // 3. Enviar a la WebApi
            return await _httpClient.GuardarOActualizarPaciente(paciente);
        }
    }
}
