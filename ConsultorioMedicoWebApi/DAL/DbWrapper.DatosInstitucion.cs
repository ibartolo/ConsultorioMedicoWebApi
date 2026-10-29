using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using Serilog;

namespace ConsultorioMedicoWebApi.DAL
{
    public partial class DbWrapper
    {
        private DatosInstitucion LlenarDatosInstitucion(IDataReader r)
        {
            return new DatosInstitucion
            {
                Id = Convert.ToInt32(r["Id"]),
                Nombre = r["Nombre"] as string,
                NoTrabajadores = Convert.IsDBNull(r["NoTrabajadores"]) ? (int?)null : Convert.ToInt32(r["NoTrabajadores"]),
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
                LogoRuta = r["LogoRuta"] as string,
                MedicoNombre = r["MedicoNombre"] as string,
                MedicoCedula = r["MedicoCedula"] as string,
                MedicoEspecialidad = r["MedicoEspecialidad"] as string
            };
        }

        /// <summary>Obtiene los datos de una institución. Reutiliza el SP existente GetDatosInstitucion.</summary>
        public ModelResponse<DatosInstitucion> ObtenerDatosInstitucion(int id, string usuario)
        {
            var mr = new ModelResponse<DatosInstitucion>();
            try
            {
                var datos = GetObject("GetDatosInstitucion", LlenarDatosInstitucion, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (datos == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No se encontraron datos de la institución.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = datos;
                mr.Message = "Datos de la institución obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener datos de la institución {Id}", id);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los datos de la institución.";
            }
            return mr;
        }

        /// <summary>Guarda o actualiza los datos de la institución. Reutiliza SaveOrUpdateDatosInstitucion.</summary>
        public ModelResponse<DatosInstitucion> GuardarDatosInstitucion(DatosInstitucion d, string usuario)
        {
            var mr = new ModelResponse<DatosInstitucion>();
            try
            {
                var pars = new List<SqlParameter>
                {
                    new SqlParameter("@Id", d.Id),
                    new SqlParameter("@Nombre", (object)d.Nombre ?? DBNull.Value),
                    new SqlParameter("@NoTrabajadores", (object)d.NoTrabajadores ?? DBNull.Value),
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
                    new SqlParameter("@MedicoNombre", (object)d.MedicoNombre ?? DBNull.Value),
                    new SqlParameter("@MedicoCedula", (object)d.MedicoCedula ?? DBNull.Value),
                    new SqlParameter("@MedicoEspecialidad", (object)d.MedicoEspecialidad ?? DBNull.Value)
                };

                var id = ExecuteScalar("SaveOrUpdateDatosInstitucion", CommandType.StoredProcedure, pars);
                if (id == null || Convert.ToInt32(id) == 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No fue posible guardar los datos de la institución.";
                    return mr;
                }

                d.Id = Convert.ToInt32(id);
                mr.IsSuccess = true;
                mr.Response = d;
                mr.Message = "Datos de la institución guardados correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar datos de la institución para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar los datos de la institución.";
            }
            return mr;
        }

        /// <summary>Actualiza únicamente la ruta del logo de la institución. SP: sp_nh_UpdateLogoInstitucion.</summary>
        public bool ActualizarLogoInstitucion(int id, string logoRuta)
        {
            try
            {
                var filas = ExecuteScalar("sp_nh_UpdateLogoInstitucion", CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@LogoRuta", (object)logoRuta ?? DBNull.Value)
                    });

                return filas != null && Convert.ToInt32(filas) > 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al actualizar logo de la institución {Id}", id);
                return false;
            }
        }
    }
}
