using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
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

        public ModelResponse<List<Tratamiento>> ObtenerTratamientos(string usuario, int empresaId)
        {
            try
            {
                Log.Information("TratamientoService.ObtenerTratamientos ENTRADA: {Json}", LogJson.ToJson(new { usuario, empresaId }));

                Log.Debug("TratamientoService.ObtenerTratamientos DB_REQUEST: {Json}", LogJson.ToJson(new { usuario, empresaId }));
                var result = _dbWrapper.ObtenerTratamientos(usuario, empresaId);
                Log.Debug("TratamientoService.ObtenerTratamientos DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("TratamientoService.ObtenerTratamientos RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "TratamientoService.ObtenerTratamientos ERROR: {Message}", ex.Message);
                return new ModelResponse<List<Tratamiento>> { IsSuccess = false, Message = "Ocurrió un error al obtener los tratamientos." };
            }
        }

        public ModelResponse<Tratamiento> ObtenerTratamientoPorId(long id, string usuario, int empresaId)
        {
            try
            {
                Log.Information("TratamientoService.ObtenerTratamientoPorId ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                Log.Debug("TratamientoService.ObtenerTratamientoPorId DB_REQUEST: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var result = _dbWrapper.ObtenerTratamientoPorId(id, usuario, empresaId);
                Log.Debug("TratamientoService.ObtenerTratamientoPorId DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("TratamientoService.ObtenerTratamientoPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "TratamientoService.ObtenerTratamientoPorId ERROR: {Message}", ex.Message);
                return new ModelResponse<Tratamiento> { IsSuccess = false, Message = "Ocurrió un error al obtener el tratamiento." };
            }
        }

        public ModelResponse<Tratamiento> GuardarOActualizarTratamiento(Tratamiento tratamiento, string usuario, int empresaId)
        {
            try
            {
                Log.Information("TratamientoService.GuardarOActualizarTratamiento ENTRADA: {Json}", LogJson.ToJson(new { tratamiento, usuario, empresaId }));

                // 1. Validación de negocio (garantiza integridad, también para app móvil)
                var error = TratamientoValidator.Validar(tratamiento);
                if (error != null)
                {
                    Log.Warning("TratamientoService.GuardarOActualizarTratamiento VALIDACIÓN: {Mensaje}", error);
                    return new ModelResponse<Tratamiento> { IsSuccess = false, Message = error };
                }

                // 2. Lógica de alta
                if (tratamiento.Id == 0)
                    tratamiento.Estatus = true;

                // 3. El tenant SIEMPRE se asigna desde el claim (nunca del cliente).
                tratamiento.ConsultorioID = empresaId;

                Log.Debug("TratamientoService.GuardarOActualizarTratamiento DB_REQUEST: {Json}", LogJson.ToJson(new { tratamiento, usuario, empresaId }));
                var result = _dbWrapper.GuardarOActualizarTratamiento(tratamiento, usuario, empresaId);
                Log.Debug("TratamientoService.GuardarOActualizarTratamiento DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("TratamientoService.GuardarOActualizarTratamiento RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "TratamientoService.GuardarOActualizarTratamiento ERROR: {Message}", ex.Message);
                return new ModelResponse<Tratamiento> { IsSuccess = false, Message = "Ocurrió un error al guardar el tratamiento." };
            }
        }
    }
}
