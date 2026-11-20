using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
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
                Log.Information("HistorialService.ObtenerTratamientosPorPaciente ENTRADA: {Json}", LogJson.ToJson(new { pacienteId, usuario, empresaId }));

                if (pacienteId <= 0)
                {
                    Log.Warning("HistorialService.ObtenerTratamientosPorPaciente VALIDACIÓN: {Mensaje}", "El identificador del paciente es inválido.");
                    return new ModelResponse<List<TratamientoHistoricoDTO>> { IsSuccess = false, Message = "El identificador del paciente es inválido." };
                }

                Log.Debug("HistorialService.ObtenerTratamientosPorPaciente DB_REQUEST: {Json}", LogJson.ToJson(new { pacienteId, usuario, empresaId }));
                var result = _dbWrapper.ObtenerTratamientosPorPaciente(pacienteId, usuario, empresaId);
                Log.Debug("HistorialService.ObtenerTratamientosPorPaciente DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("HistorialService.ObtenerTratamientosPorPaciente RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HistorialService.ObtenerTratamientosPorPaciente ERROR: {Message}", ex.Message);
                return new ModelResponse<List<TratamientoHistoricoDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener el historial de tratamientos." };
            }
        }

        public ModelResponse<List<PacientePaqueteLecturaDTO>> ObtenerPaquetesPorPaciente(int pacienteId, string usuario, int empresaId)
        {
            try
            {
                Log.Information("HistorialService.ObtenerPaquetesPorPaciente ENTRADA: {Json}", LogJson.ToJson(new { pacienteId, usuario, empresaId }));

                if (pacienteId <= 0)
                {
                    Log.Warning("HistorialService.ObtenerPaquetesPorPaciente VALIDACIÓN: {Mensaje}", "El identificador del paciente es inválido.");
                    return new ModelResponse<List<PacientePaqueteLecturaDTO>> { IsSuccess = false, Message = "El identificador del paciente es inválido." };
                }

                Log.Debug("HistorialService.ObtenerPaquetesPorPaciente DB_REQUEST: {Json}", LogJson.ToJson(new { pacienteId, usuario, empresaId }));
                var result = _dbWrapper.ObtenerPaquetesPorPaciente(pacienteId, usuario, empresaId);
                Log.Debug("HistorialService.ObtenerPaquetesPorPaciente DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("HistorialService.ObtenerPaquetesPorPaciente RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HistorialService.ObtenerPaquetesPorPaciente ERROR: {Message}", ex.Message);
                return new ModelResponse<List<PacientePaqueteLecturaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener los paquetes del paciente." };
            }
        }

        public ModelResponse<List<CitaPaqueteLecturaDTO>> ObtenerCitasPorPacientePaquete(int pacientePaqueteId, string usuario, int empresaId)
        {
            try
            {
                Log.Information("HistorialService.ObtenerCitasPorPacientePaquete ENTRADA: {Json}", LogJson.ToJson(new { pacientePaqueteId, usuario, empresaId }));

                if (pacientePaqueteId <= 0)
                {
                    Log.Warning("HistorialService.ObtenerCitasPorPacientePaquete VALIDACIÓN: {Mensaje}", "El identificador del paquete es inválido.");
                    return new ModelResponse<List<CitaPaqueteLecturaDTO>> { IsSuccess = false, Message = "El identificador del paquete es inválido." };
                }

                Log.Debug("HistorialService.ObtenerCitasPorPacientePaquete DB_REQUEST: {Json}", LogJson.ToJson(new { pacientePaqueteId, usuario, empresaId }));
                var result = _dbWrapper.ObtenerCitasPorPacientePaquete(pacientePaqueteId, usuario, empresaId);
                Log.Debug("HistorialService.ObtenerCitasPorPacientePaquete DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("HistorialService.ObtenerCitasPorPacientePaquete RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HistorialService.ObtenerCitasPorPacientePaquete ERROR: {Message}", ex.Message);
                return new ModelResponse<List<CitaPaqueteLecturaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener las citas del paquete." };
            }
        }
    }
}
