using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
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

        public ModelResponse<List<CitaDTO>> ObtenerCitas(string usuario, int empresaId)
        {
            try
            {
                Log.Information("CitaService.ObtenerCitas ENTRADA: {Json}", LogJson.ToJson(new { usuario, empresaId }));

                Log.Debug("CitaService.ObtenerCitas DB_REQUEST: {Json}", LogJson.ToJson(new { usuario, empresaId }));
                var result = _dbWrapper.ObtenerCitas(usuario, empresaId);
                Log.Debug("CitaService.ObtenerCitas DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("CitaService.ObtenerCitas RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CitaService.ObtenerCitas ERROR: {Message}", ex.Message);
                return new ModelResponse<List<CitaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener las citas." };
            }
        }

        public ModelResponse<Cita> ObtenerCitaPorId(long id, string usuario, int empresaId)
        {
            try
            {
                Log.Information("CitaService.ObtenerCitaPorId ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                Log.Debug("CitaService.ObtenerCitaPorId DB_REQUEST: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var result = _dbWrapper.ObtenerCitaPorId(id, usuario, empresaId);
                Log.Debug("CitaService.ObtenerCitaPorId DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("CitaService.ObtenerCitaPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CitaService.ObtenerCitaPorId ERROR: {Message}", ex.Message);
                return new ModelResponse<Cita> { IsSuccess = false, Message = "Ocurrió un error al obtener la cita." };
            }
        }

        public ModelResponse<Cita> GuardarOActualizarCita(Cita cita, string usuario, int empresaId)
        {
            try
            {
                Log.Information("CitaService.GuardarOActualizarCita ENTRADA: {Json}", LogJson.ToJson(new { cita, usuario, empresaId }));

                // 1. Validación de negocio (garantiza integridad, también para app móvil)
                var error = CitaValidator.Validar(cita);
                if (error != null)
                {
                    Log.Warning("CitaService.GuardarOActualizarCita VALIDACIÓN: {Mensaje}", error);
                    return new ModelResponse<Cita> { IsSuccess = false, Message = error };
                }

                // 2. Lógica de alta
                if (cita.Id == 0)
                    cita.Estatus = true;

                // 3. El tenant SIEMPRE se asigna desde el claim (nunca del cliente).
                cita.ConsultorioID = empresaId;

                Log.Debug("CitaService.GuardarOActualizarCita DB_REQUEST: {Json}", LogJson.ToJson(new { cita, usuario, empresaId }));
                var result = _dbWrapper.GuardarOActualizarCita(cita, usuario, empresaId);
                Log.Debug("CitaService.GuardarOActualizarCita DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("CitaService.GuardarOActualizarCita RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CitaService.GuardarOActualizarCita ERROR: {Message}", ex.Message);
                return new ModelResponse<Cita> { IsSuccess = false, Message = "Ocurrió un error al guardar la cita." };
            }
        }

        public ModelResponse<bool> EliminarCita(int id, string usuario, int empresaId)
        {
            try
            {
                Log.Information("CitaService.EliminarCita ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                if (id <= 0)
                {
                    Log.Warning("CitaService.EliminarCita VALIDACIÓN: {Mensaje}", "El identificador de la cita es inválido.");
                    return new ModelResponse<bool> { IsSuccess = false, Message = "El identificador de la cita es inválido." };
                }

                Log.Debug("CitaService.EliminarCita DB_REQUEST: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var result = _dbWrapper.EliminarCita(id, usuario, empresaId);
                Log.Debug("CitaService.EliminarCita DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("CitaService.EliminarCita RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CitaService.EliminarCita ERROR: {Message}", ex.Message);
                return new ModelResponse<bool> { IsSuccess = false, Message = "Ocurrió un error al eliminar la cita." };
            }
        }

        public ModelResponse<List<CitaDTO>> ObtenerCitasPorPaciente(long pacienteId, string usuario, int empresaId)
        {
            try
            {
                Log.Information("CitaService.ObtenerCitasPorPaciente ENTRADA: {Json}", LogJson.ToJson(new { pacienteId, usuario, empresaId }));

                Log.Debug("CitaService.ObtenerCitasPorPaciente DB_REQUEST: {Json}", LogJson.ToJson(new { pacienteId, usuario, empresaId }));
                var result = _dbWrapper.ObtenerCitasPorPaciente(pacienteId, usuario, empresaId);
                Log.Debug("CitaService.ObtenerCitasPorPaciente DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("CitaService.ObtenerCitasPorPaciente RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CitaService.ObtenerCitasPorPaciente ERROR: {Message}", ex.Message);
                return new ModelResponse<List<CitaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener las citas." };
            }
        }
    }
}
