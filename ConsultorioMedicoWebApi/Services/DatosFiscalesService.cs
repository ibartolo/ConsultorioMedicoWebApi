using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
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
                return _dbWrapper.ObtenerDatosFiscales(empresaId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerDatosFiscales");
                return new ModelResponse<List<DatosFiscales>> { IsSuccess = false, Message = "Ocurrió un error al obtener los datos fiscales." };
            }
        }

        public ModelResponse<DatosFiscales> ObtenerDatosFiscalActual(int empresaId, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerDatosFiscalActual(empresaId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerDatosFiscalActual");
                return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = "Ocurrió un error al obtener el dato fiscal." };
            }
        }

        public ModelResponse<DatosFiscales> ObtenerDatosFiscalPorId(int id, int empresaId, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerDatosFiscalPorId(id, empresaId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerDatosFiscalPorId");
                return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = "Ocurrió un error al obtener el dato fiscal." };
            }
        }

        public ModelResponse<DatosFiscales> GuardarDatosFiscal(DatosFiscales datos, int empresaId, string usuario)
        {
            try
            {
                var error = DatosFiscalesValidator.Validar(datos);
                if (error != null)
                    return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = error };

                if (datos.Id < 0) datos.Id = 0;

                // El consultorio (@ConsultorioID = DatosInstitucionID) SIEMPRE viene del claim.
                datos.DatosInstitucionID = empresaId;

                return _dbWrapper.GuardarDatosFiscal(datos, empresaId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarDatosFiscal");
                return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = "Ocurrió un error al guardar el dato fiscal." };
            }
        }

        public ModelResponse<bool> EliminarDatosFiscal(int id, int empresaId, string usuario)
        {
            try
            {
                return _dbWrapper.EliminarDatosFiscal(id, empresaId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EliminarDatosFiscal");
                return new ModelResponse<bool> { IsSuccess = false, Message = "Ocurrió un error al eliminar el dato fiscal." };
            }
        }
    }
}
