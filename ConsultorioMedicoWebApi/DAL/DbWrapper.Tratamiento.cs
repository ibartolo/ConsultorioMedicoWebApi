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
        private Tratamiento LlenarTratamiento(IDataReader r)
        {
            return new Tratamiento
            {
                Id = Convert.ToInt32(r["Id"]),
                Nombre = r["Nombre"] as string,
                Duracion = Convert.IsDBNull(r["Duracion"]) ? (int?)null : Convert.ToInt32(r["Duracion"]),
                Descripcion = r["Descripcion"] as string,
                Estatus = Convert.IsDBNull(r["Estatus"]) ? false : Convert.ToBoolean(r["Estatus"])
            };
        }

        /// <summary>Lista tratamientos activos. Reutiliza el SP existente GetAllTratamiento.</summary>
        public ModelResponse<List<Tratamiento>> ObtenerTratamientos(string usuario)
        {
            var mr = new ModelResponse<List<Tratamiento>>();
            try
            {
                var tratamientos = GetObjects("GetAllTratamiento", LlenarTratamiento, CommandType.StoredProcedure);
                mr.IsSuccess = true;
                mr.Response = tratamientos.ToList();
                mr.Message = "Tratamientos obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tratamientos para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los tratamientos.";
            }
            return mr;
        }

        /// <summary>Obtiene un tratamiento por Id. SP nuevo: sp_nh_GetTratamientoById.</summary>
        public ModelResponse<Tratamiento> ObtenerTratamientoPorId(long id, string usuario)
        {
            var mr = new ModelResponse<Tratamiento>();
            try
            {
                var tratamiento = GetObject("sp_nh_GetTratamientoById", LlenarTratamiento, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (tratamiento == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Tratamiento no encontrado.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = tratamiento;
                mr.Message = "Tratamiento obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tratamiento {Id}", id);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el tratamiento.";
            }
            return mr;
        }

        /// <summary>Guarda o actualiza un tratamiento (upsert). SP nuevo: sp_nh_SaveTratamiento. Devuelve el Id.</summary>
        public ModelResponse<Tratamiento> GuardarOActualizarTratamiento(Tratamiento t, string usuario)
        {
            var mr = new ModelResponse<Tratamiento>();
            try
            {
                var pars = new List<SqlParameter>
                {
                    new SqlParameter("@Id", t.Id),
                    new SqlParameter("@Nombre", (object)t.Nombre ?? DBNull.Value),
                    new SqlParameter("@Duracion", (object)t.Duracion ?? DBNull.Value),
                    new SqlParameter("@Descripcion", (object)t.Descripcion ?? DBNull.Value),
                    new SqlParameter("@Estatus", t.Estatus)
                };

                var id = ExecuteScalar("sp_nh_SaveTratamiento", CommandType.StoredProcedure, pars);
                if (id == null || Convert.ToInt64(id) == 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No fue posible guardar el tratamiento.";
                    return mr;
                }

                t.Id = Convert.ToInt32(id);
                mr.IsSuccess = true;
                mr.Response = t;
                mr.Message = "Tratamiento guardado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar tratamiento para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar el tratamiento.";
            }
            return mr;
        }
    }
}
