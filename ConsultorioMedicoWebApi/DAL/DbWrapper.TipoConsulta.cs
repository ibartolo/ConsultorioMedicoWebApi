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
        private TipoConsulta LlenarTipoConsulta(IDataReader r)
        {
            return new TipoConsulta
            {
                Id = Convert.ToInt32(r["Id"]),
                Nombre = r["Nombre"] as string,
                Duracion = Convert.IsDBNull(r["Duracion"]) ? (int?)null : Convert.ToInt32(r["Duracion"]),
                Descripcion = r["Descripcion"] as string,
                Estatus = Convert.IsDBNull(r["Estatus"]) ? false : Convert.ToBoolean(r["Estatus"])
            };
        }

        /// <summary>Lista tipos de consulta. Reutiliza el SP GetAllTipoConsulta. Filtra por tenant y por Estatus (según soloActivos).</summary>
        public ModelResponse<List<TipoConsulta>> ObtenerTiposConsulta(string usuario, int empresaId, bool soloActivos)
        {
            var mr = new ModelResponse<List<TipoConsulta>>();
            try
            {
                var tipos = GetObjects("GetAllTipoConsulta", LlenarTipoConsulta, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@ConsultorioID", empresaId), new SqlParameter("@SoloActivos", soloActivos) });

                mr.IsSuccess = true;
                mr.Response = tipos.ToList();
                mr.Message = "Tipos de consulta obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tipos de consulta para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los tipos de consulta.";
            }
            return mr;
        }

        /// <summary>Obtiene un tipo de consulta por Id. SP nuevo: sp_nh_GetTipoConsultaById. Filtra por tenant.</summary>
        public ModelResponse<TipoConsulta> ObtenerTipoConsultaPorId(long id, string usuario, int empresaId)
        {
            var mr = new ModelResponse<TipoConsulta>();
            try
            {
                var tipo = GetObject("sp_nh_GetTipoConsultaById", LlenarTipoConsulta, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id), new SqlParameter("@ConsultorioID", empresaId) });

                if (tipo == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Tipo de consulta no encontrado.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = tipo;
                mr.Message = "Tipo de consulta obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tipo de consulta {Id}", id);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el tipo de consulta.";
            }
            return mr;
        }

        /// <summary>Guarda o actualiza un tipo de consulta (upsert). SP nuevo: sp_nh_SaveTipoConsulta. Devuelve el Id. El tenant se fija por claim.</summary>
        public ModelResponse<TipoConsulta> GuardarOActualizarTipoConsulta(TipoConsulta t, string usuario, int empresaId)
        {
            var mr = new ModelResponse<TipoConsulta>();
            try
            {
                var pars = new List<SqlParameter>
                {
                    new SqlParameter("@Id", t.Id),
                    new SqlParameter("@Nombre", (object)t.Nombre ?? DBNull.Value),
                    new SqlParameter("@Duracion", (object)t.Duracion ?? DBNull.Value),
                    new SqlParameter("@Descripcion", (object)t.Descripcion ?? DBNull.Value),
                    new SqlParameter("@Estatus", t.Estatus),
                    new SqlParameter("@ConsultorioID", empresaId)
                };

                var id = ExecuteScalar("sp_nh_SaveTipoConsulta", CommandType.StoredProcedure, pars);
                if (id == null || id == DBNull.Value || Convert.ToInt64(id) == 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No fue posible guardar el tipo de consulta.";
                    return mr;
                }

                t.Id = Convert.ToInt32(id);
                mr.IsSuccess = true;
                mr.Response = t;
                mr.Message = "Tipo de consulta guardado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar tipo de consulta para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar el tipo de consulta.";
            }
            return mr;
        }

        /// <summary>Baja lógica de un tipo de consulta (Estatus=0). SP nuevo: sp_nh_DeleteTipoConsulta. Acotado al tenant.</summary>
        public ModelResponse<bool> EliminarTipoConsulta(int id, string usuario, int empresaId)
        {
            var mr = new ModelResponse<bool>();
            try
            {
                var filas = ExecuteNonQuery("sp_nh_DeleteTipoConsulta", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id), new SqlParameter("@ConsultorioID", empresaId) });

                if (filas <= 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Tipo de consulta no encontrado.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = true;
                mr.Message = "Tipo de consulta eliminado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar tipo de consulta {Id} para {Usuario}", id, usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al eliminar el tipo de consulta.";
            }
            return mr;
        }
    }
}
