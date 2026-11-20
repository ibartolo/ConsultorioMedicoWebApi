using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class TipoConsultaService
    {
        private readonly DbWrapper _dbWrapper;

        public TipoConsultaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<TipoConsulta>> ObtenerTiposConsulta(string usuario, int empresaId)
        {
            try
            {
                Log.Information("TipoConsultaService.ObtenerTiposConsulta ENTRADA: {Json}", LogJson.ToJson(new { usuario, empresaId, soloActivos = true }));

                Log.Debug("TipoConsultaService.ObtenerTiposConsulta DB_REQUEST: {Json}", LogJson.ToJson(new { usuario, empresaId, soloActivos = true }));
                var result = _dbWrapper.ObtenerTiposConsulta(usuario, empresaId, soloActivos: true);
                Log.Debug("TipoConsultaService.ObtenerTiposConsulta DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("TipoConsultaService.ObtenerTiposConsulta RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "TipoConsultaService.ObtenerTiposConsulta ERROR: {Message}", ex.Message);
                return new ModelResponse<List<TipoConsulta>> { IsSuccess = false, Message = "Ocurrió un error al obtener los tipos de consulta." };
            }
        }

        public ModelResponse<List<TipoConsulta>> ObtenerTiposConsultaAdmin(string usuario, int empresaId)
        {
            try
            {
                Log.Information("TipoConsultaService.ObtenerTiposConsultaAdmin ENTRADA: {Json}", LogJson.ToJson(new { usuario, empresaId, soloActivos = false }));

                Log.Debug("TipoConsultaService.ObtenerTiposConsultaAdmin DB_REQUEST: {Json}", LogJson.ToJson(new { usuario, empresaId, soloActivos = false }));
                var result = _dbWrapper.ObtenerTiposConsulta(usuario, empresaId, soloActivos: false);
                Log.Debug("TipoConsultaService.ObtenerTiposConsultaAdmin DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("TipoConsultaService.ObtenerTiposConsultaAdmin RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "TipoConsultaService.ObtenerTiposConsultaAdmin ERROR: {Message}", ex.Message);
                return new ModelResponse<List<TipoConsulta>> { IsSuccess = false, Message = "Ocurrió un error al obtener los tipos de consulta." };
            }
        }

        public ModelResponse<TipoConsulta> ObtenerTipoConsultaPorId(long id, string usuario, int empresaId)
        {
            try
            {
                Log.Information("TipoConsultaService.ObtenerTipoConsultaPorId ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                Log.Debug("TipoConsultaService.ObtenerTipoConsultaPorId DB_REQUEST: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var result = _dbWrapper.ObtenerTipoConsultaPorId(id, usuario, empresaId);
                Log.Debug("TipoConsultaService.ObtenerTipoConsultaPorId DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("TipoConsultaService.ObtenerTipoConsultaPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "TipoConsultaService.ObtenerTipoConsultaPorId ERROR: {Message}", ex.Message);
                return new ModelResponse<TipoConsulta> { IsSuccess = false, Message = "Ocurrió un error al obtener el tipo de consulta." };
            }
        }

        public ModelResponse<TipoConsulta> GuardarOActualizarTipoConsulta(TipoConsulta tipoConsulta, string usuario, int empresaId)
        {
            try
            {
                Log.Information("TipoConsultaService.GuardarOActualizarTipoConsulta ENTRADA: {Json}", LogJson.ToJson(new { tipoConsulta, usuario, empresaId }));

                var error = TipoConsultaValidator.Validar(tipoConsulta);
                if (error != null)
                {
                    Log.Warning("TipoConsultaService.GuardarOActualizarTipoConsulta VALIDACIÓN: {Mensaje}", error);
                    return new ModelResponse<TipoConsulta> { IsSuccess = false, Message = error };
                }

                if (tipoConsulta.Id == 0)
                    tipoConsulta.Estatus = true;

                // El tenant SIEMPRE se asigna desde el claim (nunca del cliente).
                tipoConsulta.ConsultorioID = empresaId;

                Log.Debug("TipoConsultaService.GuardarOActualizarTipoConsulta DB_REQUEST: {Json}", LogJson.ToJson(new { tipoConsulta, usuario, empresaId }));
                var result = _dbWrapper.GuardarOActualizarTipoConsulta(tipoConsulta, usuario, empresaId);
                Log.Debug("TipoConsultaService.GuardarOActualizarTipoConsulta DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("TipoConsultaService.GuardarOActualizarTipoConsulta RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "TipoConsultaService.GuardarOActualizarTipoConsulta ERROR: {Message}", ex.Message);
                return new ModelResponse<TipoConsulta> { IsSuccess = false, Message = "Ocurrió un error al guardar el tipo de consulta." };
            }
        }

        public ModelResponse<bool> EliminarTipoConsulta(int id, string usuario, int empresaId)
        {
            try
            {
                Log.Information("TipoConsultaService.EliminarTipoConsulta ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                if (id <= 0)
                {
                    Log.Warning("TipoConsultaService.EliminarTipoConsulta VALIDACIÓN: {Mensaje}", "El identificador del tipo de consulta es inválido.");
                    return new ModelResponse<bool> { IsSuccess = false, Message = "El identificador del tipo de consulta es inválido." };
                }

                Log.Debug("TipoConsultaService.EliminarTipoConsulta DB_REQUEST: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var result = _dbWrapper.EliminarTipoConsulta(id, usuario, empresaId);
                Log.Debug("TipoConsultaService.EliminarTipoConsulta DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("TipoConsultaService.EliminarTipoConsulta RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "TipoConsultaService.EliminarTipoConsulta ERROR: {Message}", ex.Message);
                return new ModelResponse<bool> { IsSuccess = false, Message = "Ocurrió un error al eliminar el tipo de consulta." };
            }
        }
    }
}
