using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class DatosFiscalesService
    {
        private readonly DbWrapper _dbWrapper;

        public DatosFiscalesService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<DatosFiscales>> ObtenerDatosFiscales(int empresaId, string usuario)
        {
            try
            {
                Log.Information("DatosFiscalesService.ObtenerDatosFiscales ENTRADA: {Json}", LogJson.ToJson(new { empresaId, usuario }));

                Log.Debug("DatosFiscalesService.ObtenerDatosFiscales DB_REQUEST: {Json}", LogJson.ToJson(new { empresaId, usuario }));
                var result = _dbWrapper.ObtenerDatosFiscales(empresaId, usuario);
                Log.Debug("DatosFiscalesService.ObtenerDatosFiscales DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("DatosFiscalesService.ObtenerDatosFiscales RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DatosFiscalesService.ObtenerDatosFiscales ERROR: {Message}", ex.Message);
                return new ModelResponse<List<DatosFiscales>> { IsSuccess = false, Message = "Ocurrió un error al obtener los datos fiscales." };
            }
        }

        public ModelResponse<DatosFiscales> ObtenerDatosFiscalActual(int empresaId, string usuario)
        {
            try
            {
                Log.Information("DatosFiscalesService.ObtenerDatosFiscalActual ENTRADA: {Json}", LogJson.ToJson(new { empresaId, usuario }));

                Log.Debug("DatosFiscalesService.ObtenerDatosFiscalActual DB_REQUEST: {Json}", LogJson.ToJson(new { empresaId, usuario }));
                var result = _dbWrapper.ObtenerDatosFiscalActual(empresaId, usuario);
                Log.Debug("DatosFiscalesService.ObtenerDatosFiscalActual DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("DatosFiscalesService.ObtenerDatosFiscalActual RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DatosFiscalesService.ObtenerDatosFiscalActual ERROR: {Message}", ex.Message);
                return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = "Ocurrió un error al obtener el dato fiscal." };
            }
        }

        public ModelResponse<DatosFiscales> ObtenerDatosFiscalPorId(int id, int empresaId, string usuario)
        {
            try
            {
                Log.Information("DatosFiscalesService.ObtenerDatosFiscalPorId ENTRADA: {Json}", LogJson.ToJson(new { id, empresaId, usuario }));

                Log.Debug("DatosFiscalesService.ObtenerDatosFiscalPorId DB_REQUEST: {Json}", LogJson.ToJson(new { id, empresaId, usuario }));
                var result = _dbWrapper.ObtenerDatosFiscalPorId(id, empresaId, usuario);
                Log.Debug("DatosFiscalesService.ObtenerDatosFiscalPorId DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("DatosFiscalesService.ObtenerDatosFiscalPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DatosFiscalesService.ObtenerDatosFiscalPorId ERROR: {Message}", ex.Message);
                return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = "Ocurrió un error al obtener el dato fiscal." };
            }
        }

        public ModelResponse<DatosFiscales> GuardarDatosFiscal(DatosFiscales datos, int empresaId, string usuario)
        {
            try
            {
                Log.Information("DatosFiscalesService.GuardarDatosFiscal ENTRADA: {Json}", LogJson.ToJson(new { datos, empresaId, usuario }));

                var error = DatosFiscalesValidator.Validar(datos);
                if (error != null)
                {
                    Log.Warning("DatosFiscalesService.GuardarDatosFiscal VALIDACIÓN: {Mensaje}", error);
                    return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = error };
                }

                if (datos.Id < 0) datos.Id = 0;

                // El consultorio (@ConsultorioID = DatosInstitucionID) SIEMPRE viene del claim.
                datos.DatosInstitucionID = empresaId;

                Log.Debug("DatosFiscalesService.GuardarDatosFiscal DB_REQUEST: {Json}", LogJson.ToJson(new { datos, empresaId, usuario }));
                var result = _dbWrapper.GuardarDatosFiscal(datos, empresaId, usuario);
                Log.Debug("DatosFiscalesService.GuardarDatosFiscal DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("DatosFiscalesService.GuardarDatosFiscal RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DatosFiscalesService.GuardarDatosFiscal ERROR: {Message}", ex.Message);
                return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = "Ocurrió un error al guardar el dato fiscal." };
            }
        }

        public ModelResponse<bool> EliminarDatosFiscal(int id, int empresaId, string usuario)
        {
            try
            {
                Log.Information("DatosFiscalesService.EliminarDatosFiscal ENTRADA: {Json}", LogJson.ToJson(new { id, empresaId, usuario }));

                Log.Debug("DatosFiscalesService.EliminarDatosFiscal DB_REQUEST: {Json}", LogJson.ToJson(new { id, empresaId, usuario }));
                var result = _dbWrapper.EliminarDatosFiscal(id, empresaId, usuario);
                Log.Debug("DatosFiscalesService.EliminarDatosFiscal DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("DatosFiscalesService.EliminarDatosFiscal RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DatosFiscalesService.EliminarDatosFiscal ERROR: {Message}", ex.Message);
                return new ModelResponse<bool> { IsSuccess = false, Message = "Ocurrió un error al eliminar el dato fiscal." };
            }
        }
    }
}
