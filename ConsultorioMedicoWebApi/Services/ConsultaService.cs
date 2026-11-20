using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class ConsultaService
    {
        private readonly DbWrapper _dbWrapper;

        public ConsultaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<ConsultaDTO>> ObtenerConsultas(string usuario, int empresaId)
        {
            try
            {
                Log.Information("ConsultaService.ObtenerConsultas ENTRADA: {Json}", LogJson.ToJson(new { usuario, empresaId }));

                Log.Debug("ConsultaService.ObtenerConsultas DB_REQUEST: {Json}", LogJson.ToJson(new { usuario, empresaId }));
                var result = _dbWrapper.ObtenerConsultas(usuario, empresaId);
                Log.Debug("ConsultaService.ObtenerConsultas DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("ConsultaService.ObtenerConsultas RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ConsultaService.ObtenerConsultas ERROR: {Message}", ex.Message);
                return new ModelResponse<List<ConsultaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener las consultas." };
            }
        }

        public ModelResponse<Consulta> ObtenerConsultaPorId(long id, string usuario, int empresaId)
        {
            try
            {
                Log.Information("ConsultaService.ObtenerConsultaPorId ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                Log.Debug("ConsultaService.ObtenerConsultaPorId DB_REQUEST: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var result = _dbWrapper.ObtenerConsultaPorId(id, usuario, empresaId);
                Log.Debug("ConsultaService.ObtenerConsultaPorId DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("ConsultaService.ObtenerConsultaPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ConsultaService.ObtenerConsultaPorId ERROR: {Message}", ex.Message);
                return new ModelResponse<Consulta> { IsSuccess = false, Message = "Ocurrió un error al obtener la consulta." };
            }
        }

        public ModelResponse<Consulta> GuardarOActualizarConsulta(Consulta consulta, string usuario, int empresaId)
        {
            try
            {
                Log.Information("ConsultaService.GuardarOActualizarConsulta ENTRADA: {Json}", LogJson.ToJson(new { consulta, usuario, empresaId }));

                // 1. Validación de negocio (garantiza integridad, también para app móvil)
                var error = ConsultaValidator.Validar(consulta);
                if (error != null)
                {
                    Log.Warning("ConsultaService.GuardarOActualizarConsulta VALIDACIÓN: {Mensaje}", error);
                    return new ModelResponse<Consulta> { IsSuccess = false, Message = error };
                }

                // 2. Lógica de alta
                if (consulta.Id == 0)
                    consulta.Estatus = true;

                // 3. El tenant SIEMPRE se asigna desde el claim (nunca del cliente).
                consulta.ConsultorioID = empresaId;

                Log.Debug("ConsultaService.GuardarOActualizarConsulta DB_REQUEST: {Json}", LogJson.ToJson(new { consulta, usuario, empresaId }));
                var result = _dbWrapper.GuardarOActualizarConsulta(consulta, usuario, empresaId);
                Log.Debug("ConsultaService.GuardarOActualizarConsulta DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("ConsultaService.GuardarOActualizarConsulta RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ConsultaService.GuardarOActualizarConsulta ERROR: {Message}", ex.Message);
                return new ModelResponse<Consulta> { IsSuccess = false, Message = "Ocurrió un error al guardar la consulta." };
            }
        }

        public ModelResponse<RecetaDTO> ObtenerRecetaPorConsulta(long consultaId, int empresaId, string usuario)
        {
            try
            {
                Log.Information("ConsultaService.ObtenerRecetaPorConsulta ENTRADA: {Json}", LogJson.ToJson(new { consultaId, empresaId, usuario }));

                Log.Debug("ConsultaService.ObtenerRecetaPorConsulta DB_REQUEST: {Json}", LogJson.ToJson(new { consultaId, empresaId, usuario }));
                var result = _dbWrapper.ObtenerRecetaPorConsulta(consultaId, empresaId, usuario);
                Log.Debug("ConsultaService.ObtenerRecetaPorConsulta DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("ConsultaService.ObtenerRecetaPorConsulta RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, consultaId);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ConsultaService.ObtenerRecetaPorConsulta ERROR: {Message}", ex.Message);
                return new ModelResponse<RecetaDTO> { IsSuccess = false, Message = "Ocurrió un error al obtener la receta." };
            }
        }

        public ModelResponse<List<ConsultaDTO>> ObtenerConsultasPorPaciente(long pacienteId, string usuario, int empresaId)
        {
            try
            {
                Log.Information("ConsultaService.ObtenerConsultasPorPaciente ENTRADA: {Json}", LogJson.ToJson(new { pacienteId, usuario, empresaId }));

                Log.Debug("ConsultaService.ObtenerConsultasPorPaciente DB_REQUEST: {Json}", LogJson.ToJson(new { pacienteId, usuario, empresaId }));
                var result = _dbWrapper.ObtenerConsultasPorPaciente(pacienteId, usuario, empresaId);
                Log.Debug("ConsultaService.ObtenerConsultasPorPaciente DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("ConsultaService.ObtenerConsultasPorPaciente RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ConsultaService.ObtenerConsultasPorPaciente ERROR: {Message}", ex.Message);
                return new ModelResponse<List<ConsultaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener las consultas." };
            }
        }
    }
}
