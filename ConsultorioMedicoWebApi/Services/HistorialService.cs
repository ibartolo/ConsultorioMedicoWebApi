using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class HistorialService
    {
        private readonly DbWrapper _dbWrapper;

        public HistorialService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<TratamientoHistoricoDTO>> ObtenerTratamientosPorPaciente(long pacienteId, string usuario)
        {
            try
            {
                if (pacienteId <= 0)
                    return new ModelResponse<List<TratamientoHistoricoDTO>> { IsSuccess = false, Message = "El identificador del paciente es inválido." };

                return _dbWrapper.ObtenerTratamientosPorPaciente(pacienteId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerTratamientosPorPaciente");
                return new ModelResponse<List<TratamientoHistoricoDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener el historial de tratamientos." };
            }
        }
    }
}
