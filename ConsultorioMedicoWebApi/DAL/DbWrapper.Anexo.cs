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
        private Anexo LlenarAnexoLista(IDataReader r)
        {
            return new Anexo
            {
                Id = Convert.ToInt32(r["Id"]),
                Estatus = Convert.ToBoolean(r["Estatus"]),
                Fecha = Convert.IsDBNull(r["Fecha"]) ? (DateTime?)null : Convert.ToDateTime(r["Fecha"]),
                Comentarios = r["Comentarios"] as string,
                Nombre = r["Nombre"] as string,
                Ruta = r["Ruta"] as string
            };
        }

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

        /// <summary>Lista anexos por paciente. Reutiliza el SP existente GetAllAnexoByPacienteID.</summary>
        public ModelResponse<List<Anexo>> ObtenerAnexosPorPaciente(long pacienteId, string usuario)
        {
            var mr = new ModelResponse<List<Anexo>>();
            try
            {
                var anexos = GetObjects("GetAllAnexoByPacienteID", LlenarAnexoLista, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@IdPaciente", pacienteId) });
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
    }
}
