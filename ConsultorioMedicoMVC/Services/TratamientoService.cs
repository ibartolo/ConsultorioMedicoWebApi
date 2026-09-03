using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class TratamientoService
    {
        private readonly HttpClientConnection _httpClient;

        public TratamientoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<Tratamiento>>> ConsultarTratamientos()
            => await _httpClient.ObtenerTratamientos();

        public async Task<Tratamiento> ObtenerTratamientoPorId(long id)
        {
            var r = await _httpClient.ObtenerTratamientoPorId(id);
            return (r.IsSuccess && r.Response != null) ? r.Response : null;
        }

        public async Task<ModelResponse<Tratamiento>> GuardarOActualizarTratamiento(Tratamiento tratamiento)
        {
            // 1. Validación de negocio (mismas reglas que jQuery Validate)
            var error = TratamientoValidator.Validar(tratamiento);
            if (error != null)
                return new ModelResponse<Tratamiento> { IsSuccess = false, Message = error };

            // 2. Lógica de alta
            if (tratamiento.Id == 0)
                tratamiento.Estatus = true;

            return await _httpClient.GuardarOActualizarTratamiento(tratamiento);
        }
    }
}
