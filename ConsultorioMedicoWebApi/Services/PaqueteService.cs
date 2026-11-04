using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
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
                return _dbWrapper.ObtenerPaquetes(usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPaquetes");
                return new ModelResponse<List<Paquete>> { IsSuccess = false, Message = "Ocurrió un error al obtener los paquetes." };
            }
        }

        public ModelResponse<Paquete> ObtenerPaquetePorId(long id, string usuario, int empresaId)
        {
            try
            {
                return _dbWrapper.ObtenerPaquetePorId(id, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPaquetePorId");
                return new ModelResponse<Paquete> { IsSuccess = false, Message = "Ocurrió un error al obtener el paquete." };
            }
        }

        public ModelResponse<Paquete> GuardarOActualizarPaquete(Paquete paquete, string usuario, int empresaId)
        {
            try
            {
                var error = PaqueteValidator.Validar(paquete);
                if (error != null)
                    return new ModelResponse<Paquete> { IsSuccess = false, Message = error };

                if (paquete.Id == 0)
                {
                    paquete.Estatus = true;
                    paquete.FechaCreacion = DateTime.Now;
                }

                // El tenant SIEMPRE se asigna desde el claim (nunca del cliente).
                paquete.ConsultorioID = empresaId;

                return _dbWrapper.GuardarOActualizarPaquete(paquete, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarOActualizarPaquete");
                return new ModelResponse<Paquete> { IsSuccess = false, Message = "Ocurrió un error al guardar el paquete." };
            }
        }
    }
}
