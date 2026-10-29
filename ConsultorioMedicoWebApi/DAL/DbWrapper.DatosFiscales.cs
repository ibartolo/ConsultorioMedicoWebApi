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
        private DatosFiscales LlenarDatosFiscal(IDataReader r)
        {
            return new DatosFiscales
            {
                Id = Convert.ToInt32(r["Id"]),
                RazonSocial = r["RazonSocial"] as string,
                RFC = r["RFC"] as string,
                Telefono1 = r["Telefono1"] as string,
                Telefono2 = r["Telefono2"] as string,
                Email = r["Email"] as string,
                Calle = r["Calle"] as string,
                NoInterior = r["NoInterior"] as string,
                NoExterior = r["NoExterior"] as string,
                Colonia = r["Colonia"] as string,
                Deleacion = r["Deleacion"] as string,
                Municipio = r["Municipio"] as string,
                Estado = r["Estado"] as string,
                CP = Convert.IsDBNull(r["CP"]) ? (int?)null : Convert.ToInt32(r["CP"]),
                Pais = r["Pais"] as string,
                DatosInstitucionID = Convert.IsDBNull(r["DatosInstitucionID"]) ? (int?)null : Convert.ToInt32(r["DatosInstitucionID"]),
                DatosInstitucionNombre = r["DatosInstitucionNombre"] as string
            };
        }

        /// <summary>Lista todos los registros fiscales. SP nuevo: sp_nh_GetDatosFiscales.</summary>
        public ModelResponse<List<DatosFiscales>> ObtenerDatosFiscales(string usuario)
        {
            var mr = new ModelResponse<List<DatosFiscales>>();
            try
            {
                var datos = GetObjects("sp_nh_GetDatosFiscales", LlenarDatosFiscal, CommandType.StoredProcedure);
                mr.IsSuccess = true;
                mr.Response = datos.ToList();
                mr.Message = "Datos fiscales obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener datos fiscales para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los datos fiscales.";
            }
            return mr;
        }

        /// <summary>Obtiene el único registro fiscal (si existe). Reutiliza sp_nh_GetDatosFiscales.</summary>
        public ModelResponse<DatosFiscales> ObtenerDatosFiscalUnico(string usuario)
        {
            var mr = new ModelResponse<DatosFiscales>();
            try
            {
                var dato = GetObjects("sp_nh_GetDatosFiscales", LlenarDatosFiscal, CommandType.StoredProcedure).FirstOrDefault();
                if (dato == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No se encontraron datos fiscales.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = dato;
                mr.Message = "Dato fiscal obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener el dato fiscal único para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el dato fiscal.";
            }
            return mr;
        }

        /// <summary>Obtiene un registro fiscal por Id. SP nuevo: sp_nh_GetDatosFiscalById.</summary>
        public ModelResponse<DatosFiscales> ObtenerDatosFiscalPorId(int id, string usuario)
        {
            var mr = new ModelResponse<DatosFiscales>();
            try
            {
                var dato = GetObject("sp_nh_GetDatosFiscalById", LlenarDatosFiscal, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (dato == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Dato fiscal no encontrado.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = dato;
                mr.Message = "Dato fiscal obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener dato fiscal {Id}", id);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el dato fiscal.";
            }
            return mr;
        }

        /// <summary>Guarda o actualiza un registro fiscal. SP nuevo: sp_nh_SaveDatosFiscal.</summary>
        public ModelResponse<DatosFiscales> GuardarDatosFiscal(DatosFiscales d, string usuario)
        {
            var mr = new ModelResponse<DatosFiscales>();
            try
            {
                var pars = new List<SqlParameter>
                {
                    new SqlParameter("@Id", d.Id),
                    new SqlParameter("@RazonSocial", (object)d.RazonSocial ?? DBNull.Value),
                    new SqlParameter("@RFC", (object)d.RFC ?? DBNull.Value),
                    new SqlParameter("@Telefono1", (object)d.Telefono1 ?? DBNull.Value),
                    new SqlParameter("@Telefono2", (object)d.Telefono2 ?? DBNull.Value),
                    new SqlParameter("@Email", (object)d.Email ?? DBNull.Value),
                    new SqlParameter("@Calle", (object)d.Calle ?? DBNull.Value),
                    new SqlParameter("@NoInterior", (object)d.NoInterior ?? DBNull.Value),
                    new SqlParameter("@NoExterior", (object)d.NoExterior ?? DBNull.Value),
                    new SqlParameter("@Colonia", (object)d.Colonia ?? DBNull.Value),
                    new SqlParameter("@Deleacion", (object)d.Deleacion ?? DBNull.Value),
                    new SqlParameter("@Municipio", (object)d.Municipio ?? DBNull.Value),
                    new SqlParameter("@Estado", (object)d.Estado ?? DBNull.Value),
                    new SqlParameter("@CP", (object)d.CP ?? DBNull.Value),
                    new SqlParameter("@Pais", (object)d.Pais ?? DBNull.Value),
                    new SqlParameter("@DatosInstitucionID", (object)d.DatosInstitucionID ?? DBNull.Value)
                };

                var id = ExecuteScalar("sp_nh_SaveDatosFiscal", CommandType.StoredProcedure, pars);
                if (id == null || Convert.ToInt32(id) == 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No fue posible guardar el dato fiscal.";
                    return mr;
                }

                d.Id = Convert.ToInt32(id);
                mr.IsSuccess = true;
                mr.Response = d;
                mr.Message = "Dato fiscal guardado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar dato fiscal para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar el dato fiscal.";
            }
            return mr;
        }

        /// <summary>Elimina un registro fiscal. SP nuevo: sp_nh_DeleteDatosFiscal.</summary>
        public ModelResponse<bool> EliminarDatosFiscal(int id, string usuario)
        {
            var mr = new ModelResponse<bool>();
            try
            {
                var filas = ExecuteScalar("sp_nh_DeleteDatosFiscal", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (filas == null || Convert.ToInt32(filas) == 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Dato fiscal no encontrado.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = true;
                mr.Message = "Dato fiscal eliminado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar dato fiscal {Id}", id);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al eliminar el dato fiscal.";
            }
            return mr;
        }
    }
}
