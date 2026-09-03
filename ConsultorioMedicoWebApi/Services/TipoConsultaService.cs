using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
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

        public ModelResponse<List<TipoConsulta>> ObtenerTiposConsulta(string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerTiposConsulta(usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerTiposConsulta");
                return new ModelResponse<List<TipoConsulta>> { IsSuccess = false, Message = "Ocurrió un error al obtener los tipos de consulta." };
            }
        }
    }
}
