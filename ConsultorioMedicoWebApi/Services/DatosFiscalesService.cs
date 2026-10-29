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

        public ModelResponse<List<DatosFiscales>> ObtenerDatosFiscales(string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerDatosFiscales(usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerDatosFiscales");
                return new ModelResponse<List<DatosFiscales>> { IsSuccess = false, Message = "Ocurrió un error al obtener los datos fiscales." };
            }
        }

        public ModelResponse<DatosFiscales> ObtenerDatosFiscalUnico(string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerDatosFiscalUnico(usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerDatosFiscalUnico");
                return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = "Ocurrió un error al obtener el dato fiscal." };
            }
        }

        public ModelResponse<DatosFiscales> ObtenerDatosFiscalPorId(int id, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerDatosFiscalPorId(id, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerDatosFiscalPorId");
                return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = "Ocurrió un error al obtener el dato fiscal." };
            }
        }

        public ModelResponse<DatosFiscales> GuardarDatosFiscal(DatosFiscales datos, string usuario)
        {
            try
            {
                var error = DatosFiscalesValidator.Validar(datos);
                if (error != null)
                    return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = error };

                if (datos.Id < 0) datos.Id = 0;
                return _dbWrapper.GuardarDatosFiscal(datos, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarDatosFiscal");
                return new ModelResponse<DatosFiscales> { IsSuccess = false, Message = "Ocurrió un error al guardar el dato fiscal." };
            }
        }

        public ModelResponse<bool> EliminarDatosFiscal(int id, string usuario)
        {
            try
            {
                return _dbWrapper.EliminarDatosFiscal(id, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EliminarDatosFiscal");
                return new ModelResponse<bool> { IsSuccess = false, Message = "Ocurrió un error al eliminar el dato fiscal." };
            }
        }
    }
}
