using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using Serilog;

namespace ConsultorioMedicoWebApi.DAL
{
    public partial class DbWrapper
    {
        /// <summary>
        /// Indicadores del mes en curso: pacientes nuevos, consultas y citas con tratamiento.
        /// Consulta inline de solo lectura (sin cambios en la BD).
        /// </summary>
        public ModelResponse<DashboardDTO> ObtenerIndicadoresDashboard(string usuario)
        {
            var mr = new ModelResponse<DashboardDTO>();
            try
            {
                var inicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var fin = inicio.AddMonths(1);

                const string sql = @"
SELECT
    (SELECT COUNT(*) FROM dbo.Paciente
      WHERE Estatus = 1
        AND ISNULL(FechaAlta, CreatedDT) >= @Inicio
        AND ISNULL(FechaAlta, CreatedDT) < @Fin) AS PacientesNuevosMes,
    (SELECT COUNT(*) FROM dbo.Consulta
      WHERE Estatus = 1
        AND FechaConsulta >= @Inicio
        AND FechaConsulta < @Fin) AS ConsultasMes,
    (SELECT COUNT(*) FROM dbo.Cita
      WHERE Estatus = 1
        AND TratamientoID IS NOT NULL
        AND FechaCita >= @Inicio
        AND FechaCita < @Fin) AS TratamientosMes";

                var indicadores = GetObject(sql,
                    new Func<IDataReader, DashboardDTO>(r => new DashboardDTO
                    {
                        PacientesNuevosMes = Convert.ToInt32(r["PacientesNuevosMes"]),
                        ConsultasMes = Convert.ToInt32(r["ConsultasMes"]),
                        TratamientosMes = Convert.ToInt32(r["TratamientosMes"]),
                        Mes = inicio.ToString("MMMM yyyy", new CultureInfo("es-MX"))
                    }),
                    CommandType.Text,
                    new[]
                    {
                        new SqlParameter("@Inicio", SqlDbType.DateTime) { Value = inicio },
                        new SqlParameter("@Fin", SqlDbType.DateTime) { Value = fin }
                    });

                if (indicadores == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No fue posible obtener los indicadores.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = indicadores;
                mr.Message = "Indicadores obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener indicadores del dashboard para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los indicadores.";
            }
            return mr;
        }
    }
}
