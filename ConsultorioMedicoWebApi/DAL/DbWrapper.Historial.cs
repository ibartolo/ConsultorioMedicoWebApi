using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using Serilog;

namespace ConsultorioMedicoWebApi.DAL
{
    public partial class DbWrapper
    {
        /// <summary>Lista el historial de tratamientos de un paciente (Cita + Consulta). SP nuevo: sp_nh_GetTratamientosPorPaciente.</summary>
        public ModelResponse<List<TratamientoHistoricoDTO>> ObtenerTratamientosPorPaciente(long pacienteId, string usuario)
        {
            var mr = new ModelResponse<List<TratamientoHistoricoDTO>>();
            try
            {
                var tratamientos = GetObjects("sp_nh_GetTratamientosPorPaciente",
                    new Func<IDataReader, TratamientoHistoricoDTO>(r => new TratamientoHistoricoDTO
                    {
                        Id = Convert.ToInt32(r["Id"]),
                        Fecha = Convert.IsDBNull(r["Fecha"]) ? (DateTime?)null : Convert.ToDateTime(r["Fecha"]),
                        TratamientoNombre = r["TratamientoNombre"] as string,
                        Origen = r["Origen"] as string,
                        Comentarios = r["Comentarios"] as string
                    }),
                    CommandType.StoredProcedure,
                    new[] { new SqlParameter("@IdPaciente", pacienteId) });

                mr.IsSuccess = true;
                mr.Response = tratamientos.ToList();
                mr.Message = "Tratamientos obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener el historial de tratamientos del paciente {Id}", pacienteId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el historial de tratamientos.";
            }
            return mr;
        }
    }
}
