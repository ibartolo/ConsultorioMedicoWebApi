using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class CitaService
    {
        private readonly DbWrapper _dbWrapper;

        public CitaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<CitaDTO>> ObtenerCitas(string usuario)
        {
            try
            {
                Log.Information("CitaService.ObtenerCitas para {Usuario}", usuario);
                return _dbWrapper.ObtenerCitas(usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerCitas");
                return new ModelResponse<List<CitaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener las citas." };
            }
        }

        public ModelResponse<Cita> ObtenerCitaPorId(long id, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerCitaPorId(id, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerCitaPorId");
                return new ModelResponse<Cita> { IsSuccess = false, Message = "Ocurrió un error al obtener la cita." };
            }
        }

        public ModelResponse<Cita> GuardarOActualizarCita(Cita cita, string usuario)
        {
            try
            {
                // 1. Validación de negocio (garantiza integridad, también para app móvil)
                var error = CitaValidator.Validar(cita);
                if (error != null)
                    return new ModelResponse<Cita> { IsSuccess = false, Message = error };

                // 2. Lógica de alta
                if (cita.Id == 0)
                    cita.Estatus = true;

                return _dbWrapper.GuardarOActualizarCita(cita, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarOActualizarCita");
                return new ModelResponse<Cita> { IsSuccess = false, Message = "Ocurrió un error al guardar la cita." };
            }
        }

        public ModelResponse<bool> EliminarCita(int id, string usuario)
        {
            try
            {
                if (id <= 0)
                    return new ModelResponse<bool> { IsSuccess = false, Message = "El identificador de la cita es inválido." };

                return _dbWrapper.EliminarCita(id, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EliminarCita");
                return new ModelResponse<bool> { IsSuccess = false, Message = "Ocurrió un error al eliminar la cita." };
            }
        }

        public ModelResponse<List<CitaDTO>> ObtenerCitasPorPaciente(long pacienteId, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerCitasPorPaciente(pacienteId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerCitasPorPaciente");
                return new ModelResponse<List<CitaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener las citas." };
            }
        }
    }
}
