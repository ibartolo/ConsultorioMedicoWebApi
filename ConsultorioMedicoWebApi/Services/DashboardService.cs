using System;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class DashboardService
    {
        private readonly DbWrapper _dbWrapper;

        public DashboardService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<DashboardDTO> ObtenerIndicadores(string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerIndicadoresDashboard(usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerIndicadores");
                return new ModelResponse<DashboardDTO> { IsSuccess = false, Message = "Ocurrió un error al obtener los indicadores." };
            }
        }
    }
}
