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

        public ModelResponse<List<TratamientoHistoricoDTO>> ObtenerTratamientosPorPaciente(long pacienteId, string usuario, int empresaId)
        {
            try
            {
                if (pacienteId <= 0)
                    return new ModelResponse<List<TratamientoHistoricoDTO>> { IsSuccess = false, Message = "El identificador del paciente es inválido." };

                return _dbWrapper.ObtenerTratamientosPorPaciente(pacienteId, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerTratamientosPorPaciente");
                return new ModelResponse<List<TratamientoHistoricoDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener el historial de tratamientos." };
            }
        }

        public ModelResponse<List<PacientePaqueteLecturaDTO>> ObtenerPaquetesPorPaciente(int pacienteId, string usuario, int empresaId)
        {
            try
            {
                if (pacienteId <= 0)
                    return new ModelResponse<List<PacientePaqueteLecturaDTO>> { IsSuccess = false, Message = "El identificador del paciente es inválido." };

                return _dbWrapper.ObtenerPaquetesPorPaciente(pacienteId, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPaquetesPorPaciente");
                return new ModelResponse<List<PacientePaqueteLecturaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener los paquetes del paciente." };
            }
        }

        public ModelResponse<List<CitaPaqueteLecturaDTO>> ObtenerCitasPorPacientePaquete(int pacientePaqueteId, string usuario, int empresaId)
        {
            try
            {
                if (pacientePaqueteId <= 0)
                    return new ModelResponse<List<CitaPaqueteLecturaDTO>> { IsSuccess = false, Message = "El identificador del paquete es inválido." };

                return _dbWrapper.ObtenerCitasPorPacientePaquete(pacientePaqueteId, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerCitasPorPacientePaquete");
                return new ModelResponse<List<CitaPaqueteLecturaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener las citas del paquete." };
            }
        }
    }
}
