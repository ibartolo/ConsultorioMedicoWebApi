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
        private Paquete LlenarPaquete(IDataReader r)
        {
            return new Paquete
            {
                Id = Convert.ToInt32(r["Id"]),
                Nombre = r["Nombre"] as string,
                Descripcion = r["Descripcion"] as string,
                FechaCreacion = Convert.IsDBNull(r["FechaCreacion"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaCreacion"]),
                Estatus = Convert.IsDBNull(r["Estatus"]) ? false : Convert.ToBoolean(r["Estatus"]),
                Costo = Convert.IsDBNull(r["Costo"]) ? (int?)null : Convert.ToInt32(r["Costo"])
            };
        }

        /// <summary>Lista paquetes activos. Reutiliza el SP existente GetAllPaqueteDT.</summary>
        public ModelResponse<List<Paquete>> ObtenerPaquetes(string usuario)
        {
            var mr = new ModelResponse<List<Paquete>>();
            try
            {
                var paquetes = GetObjects("GetAllPaqueteDT", LlenarPaquete, CommandType.StoredProcedure);
                mr.IsSuccess = true;
                mr.Response = paquetes.ToList();
                mr.Message = "Paquetes obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener paquetes para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los paquetes.";
            }
            return mr;
        }

        /// <summary>Obtiene un paquete por Id. SP nuevo: sp_nh_GetPaqueteById.</summary>
        public ModelResponse<Paquete> ObtenerPaquetePorId(long id, string usuario)
        {
            var mr = new ModelResponse<Paquete>();
            try
            {
                var paquete = GetObject("sp_nh_GetPaqueteById", LlenarPaquete, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (paquete == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Paquete no encontrado.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = paquete;
                mr.Message = "Paquete obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener paquete {Id}", id);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el paquete.";
            }
            return mr;
        }

        /// <summary>Guarda o actualiza un paquete (upsert). SP nuevo: sp_nh_SavePaquete. Devuelve el Id.</summary>
        public ModelResponse<Paquete> GuardarOActualizarPaquete(Paquete p, string usuario)
        {
            var mr = new ModelResponse<Paquete>();
            try
            {
                var pars = new List<SqlParameter>
                {
                    new SqlParameter("@Id", p.Id),
                    new SqlParameter("@Nombre", (object)p.Nombre ?? DBNull.Value),
                    new SqlParameter("@Descripcion", (object)p.Descripcion ?? DBNull.Value),
                    new SqlParameter("@FechaCreacion", (object)p.FechaCreacion ?? DBNull.Value),
                    new SqlParameter("@Estatus", p.Estatus),
                    new SqlParameter("@Costo", (object)p.Costo ?? DBNull.Value)
                };

                var id = ExecuteScalar("sp_nh_SavePaquete", CommandType.StoredProcedure, pars);
                if (id == null || Convert.ToInt64(id) == 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No fue posible guardar el paquete.";
                    return mr;
                }

                p.Id = Convert.ToInt32(id);
                mr.IsSuccess = true;
                mr.Response = p;
                mr.Message = "Paquete guardado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar paquete para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar el paquete.";
            }
            return mr;
        }
    }
}
