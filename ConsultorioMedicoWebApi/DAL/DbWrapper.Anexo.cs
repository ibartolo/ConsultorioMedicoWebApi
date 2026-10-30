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
        private Anexo LlenarAnexo(IDataReader r)
        {
            return new Anexo
            {
                Id = Convert.ToInt32(r["Id"]),
                Estatus = Convert.ToBoolean(r["Estatus"]),
                Fecha = Convert.IsDBNull(r["Fecha"]) ? (DateTime?)null : Convert.ToDateTime(r["Fecha"]),
                Comentarios = r["Comentarios"] as string,
                PacienteID = Convert.ToInt32(r["PacienteID"]),
                Nombre = r["Nombre"] as string,
                Ruta = r["Ruta"] as string,
                ConsultaID = Convert.IsDBNull(r["ConsultaID"]) ? (int?)null : Convert.ToInt32(r["ConsultaID"])
            };
        }

        /// <summary>
        /// Lista los anexos activos de un paciente incluyendo ConsultaID (para filtrar en la vista).
        /// Se usa SQL inline para no depender de un SP nuevo ni modificar la BD.
        /// </summary>
        public ModelResponse<List<Anexo>> ObtenerAnexosPorPaciente(long pacienteId, string usuario)
        {
            var mr = new ModelResponse<List<Anexo>>();
            try
            {
                var anexos = GetObjects(
                    "SELECT Id, Estatus, Fecha, Comentarios, PacienteID, Nombre, Ruta, ConsultaID " +
                    "FROM dbo.Anexo WHERE PacienteID = @PacienteId AND Estatus = 1 ORDER BY Fecha DESC, Id DESC",
                    LlenarAnexo,
                    CommandType.Text,
                    new[] { new SqlParameter("@PacienteId", pacienteId) });

                mr.IsSuccess = true;
                mr.Response = anexos.ToList();
                mr.Message = "Anexos obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener anexos del paciente {Id}", pacienteId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los anexos.";
            }
            return mr;
        }

        /// <summary>Valida que una consulta pertenezca al paciente (integridad del anexo).</summary>
        public bool ConsultaPerteneceAPaciente(long consultaId, long pacienteId)
        {
            var total = ExecuteScalar(
                "SELECT COUNT(*) FROM dbo.Consulta WHERE Id = @ConsultaId AND PacienteID = @PacienteId",
                CommandType.Text,
                new[]
                {
                    new SqlParameter("@ConsultaId", consultaId),
                    new SqlParameter("@PacienteId", pacienteId)
                });

            return total != null && Convert.ToInt32(total) > 0;
        }

        /// <summary>Obtiene un anexo por Id. SP nuevo: sp_nh_GetAnexoById.</summary>
        public ModelResponse<Anexo> ObtenerAnexoPorId(long id, string usuario)
        {
            var mr = new ModelResponse<Anexo>();
            try
            {
                var anexo = GetObject("sp_nh_GetAnexoById", LlenarAnexo, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (anexo == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Anexo no encontrado.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = anexo;
                mr.Message = "Anexo obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener anexo {Id}", id);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el anexo.";
            }
            return mr;
        }

        /// <summary>Inserta un anexo. SP nuevo: sp_nh_SaveAnexo. Devuelve el Id.</summary>
        public int GuardarAnexo(Anexo a, string usuario)
        {
            try
            {
                var pars = new List<SqlParameter>
                {
                    new SqlParameter("@Estatus", a.Estatus),
                    new SqlParameter("@Fecha", (object)a.Fecha ?? DBNull.Value),
                    new SqlParameter("@Comentarios", (object)a.Comentarios ?? DBNull.Value),
                    new SqlParameter("@PacienteID", a.PacienteID),
                    new SqlParameter("@Nombre", (object)a.Nombre ?? DBNull.Value),
                    new SqlParameter("@Ruta", (object)a.Ruta ?? DBNull.Value),
                    new SqlParameter("@ConsultaID", (object)a.ConsultaID ?? DBNull.Value)
                };

                var id = ExecuteScalar("sp_nh_SaveAnexo", CommandType.StoredProcedure, pars);
                return id == null ? 0 : Convert.ToInt32(id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar anexo para {Usuario}", usuario);
                return 0;
            }
        }

        /// <summary>Elimina el registro del anexo. El archivo físico lo borra el servicio.</summary>
        public bool EliminarAnexo(long id, string usuario)
        {
            try
            {
                var filas = ExecuteNonQuery(
                    "DELETE FROM dbo.Anexo WHERE Id = @Id",
                    CommandType.Text,
                    new[] { new SqlParameter("@Id", id) });

                return filas > 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar anexo {Id} para {Usuario}", id, usuario);
                return false;
            }
        }
    }
}
