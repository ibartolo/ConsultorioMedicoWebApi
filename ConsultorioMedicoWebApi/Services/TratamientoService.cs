using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class TratamientoService
    {
        private readonly DbWrapper _dbWrapper;

        public TratamientoService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Tratamiento>> ObtenerTratamientos(string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerTratamientos(usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerTratamientos");
                return new ModelResponse<List<Tratamiento>> { IsSuccess = false, Message = "Ocurrió un error al obtener los tratamientos." };
            }
        }

        public ModelResponse<Tratamiento> ObtenerTratamientoPorId(long id, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerTratamientoPorId(id, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerTratamientoPorId");
                return new ModelResponse<Tratamiento> { IsSuccess = false, Message = "Ocurrió un error al obtener el tratamiento." };
            }
        }

        public ModelResponse<Tratamiento> GuardarOActualizarTratamiento(Tratamiento tratamiento, string usuario)
        {
            try
            {
                // 1. Validación de negocio (garantiza integridad, también para app móvil)
                var error = TratamientoValidator.Validar(tratamiento);
                if (error != null)
                    return new ModelResponse<Tratamiento> { IsSuccess = false, Message = error };

                // 2. Lógica de alta
                if (tratamiento.Id == 0)
                    tratamiento.Estatus = true;

                return _dbWrapper.GuardarOActualizarTratamiento(tratamiento, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarOActualizarTratamiento");
                return new ModelResponse<Tratamiento> { IsSuccess = false, Message = "Ocurrió un error al guardar el tratamiento." };
            }
        }
    }
}
