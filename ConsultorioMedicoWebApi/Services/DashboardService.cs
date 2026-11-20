using System;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
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

        public ModelResponse<DashboardDTO> ObtenerIndicadores(string usuario, int empresaId)
        {
            try
            {
                Log.Information("DashboardService.ObtenerIndicadores ENTRADA: {Json}", LogJson.ToJson(new { usuario, empresaId }));

                Log.Debug("DashboardService.ObtenerIndicadores DB_REQUEST: {Json}", LogJson.ToJson(new { usuario, empresaId }));
                var result = _dbWrapper.ObtenerIndicadoresDashboard(usuario, empresaId);
                Log.Debug("DashboardService.ObtenerIndicadores DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("DashboardService.ObtenerIndicadores RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DashboardService.ObtenerIndicadores ERROR: {Message}", ex.Message);
                return new ModelResponse<DashboardDTO> { IsSuccess = false, Message = "Ocurrió un error al obtener los indicadores." };
            }
        }
    }
}
