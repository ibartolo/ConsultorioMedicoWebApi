using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class PaqueteService
    {
        private readonly DbWrapper _dbWrapper;

        public PaqueteService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Paquete>> ObtenerPaquetes(string usuario, int empresaId)
        {
            try
            {
                Log.Information("PaqueteService.ObtenerPaquetes ENTRADA: {Json}", LogJson.ToJson(new { usuario, empresaId }));

                Log.Debug("PaqueteService.ObtenerPaquetes DB_REQUEST: {Json}", LogJson.ToJson(new { usuario, empresaId }));
                var result = _dbWrapper.ObtenerPaquetes(usuario, empresaId);
                Log.Debug("PaqueteService.ObtenerPaquetes DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("PaqueteService.ObtenerPaquetes RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PaqueteService.ObtenerPaquetes ERROR: {Message}", ex.Message);
                return new ModelResponse<List<Paquete>> { IsSuccess = false, Message = "Ocurrió un error al obtener los paquetes." };
            }
        }

        public ModelResponse<Paquete> ObtenerPaquetePorId(long id, string usuario, int empresaId)
        {
            try
            {
                Log.Information("PaqueteService.ObtenerPaquetePorId ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                Log.Debug("PaqueteService.ObtenerPaquetePorId DB_REQUEST: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var result = _dbWrapper.ObtenerPaquetePorId(id, usuario, empresaId);
                Log.Debug("PaqueteService.ObtenerPaquetePorId DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("PaqueteService.ObtenerPaquetePorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PaqueteService.ObtenerPaquetePorId ERROR: {Message}", ex.Message);
                return new ModelResponse<Paquete> { IsSuccess = false, Message = "Ocurrió un error al obtener el paquete." };
            }
        }

        public ModelResponse<Paquete> GuardarOActualizarPaquete(Paquete paquete, string usuario, int empresaId)
        {
            try
            {
                Log.Information("PaqueteService.GuardarOActualizarPaquete ENTRADA: {Json}", LogJson.ToJson(new { paquete, usuario, empresaId }));

                var error = PaqueteValidator.Validar(paquete);
                if (error != null)
                {
                    Log.Warning("PaqueteService.GuardarOActualizarPaquete VALIDACIÓN: {Mensaje}", error);
                    return new ModelResponse<Paquete> { IsSuccess = false, Message = error };
                }

                if (paquete.Id == 0)
                {
                    paquete.Estatus = true;
                    paquete.FechaCreacion = DateTime.Now;
                }

                // El tenant SIEMPRE se asigna desde el claim (nunca del cliente).
                paquete.ConsultorioID = empresaId;

                Log.Debug("PaqueteService.GuardarOActualizarPaquete DB_REQUEST: {Json}", LogJson.ToJson(new { paquete, usuario, empresaId }));
                var result = _dbWrapper.GuardarOActualizarPaquete(paquete, usuario, empresaId);
                Log.Debug("PaqueteService.GuardarOActualizarPaquete DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("PaqueteService.GuardarOActualizarPaquete RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PaqueteService.GuardarOActualizarPaquete ERROR: {Message}", ex.Message);
                return new ModelResponse<Paquete> { IsSuccess = false, Message = "Ocurrió un error al guardar el paquete." };
            }
        }
    }
}
