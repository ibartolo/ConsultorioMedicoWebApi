using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ConsultorioMedicoEntities.Registro;
using ConsultorioMedicoEntities.Seguridad;
using Serilog;

namespace ConsultorioMedicoWebApi.DAL
{
    /// <summary>
    /// Registro público de consultorio (SaaS). Crea DatosInstitucion + DatosFiscales (1:1) + Usuario admin
    /// en una sola transacción. Reutiliza la transacción "ambiente" de BaseDbWrapper.
    /// </summary>
    public partial class DbWrapper
    {
        /// <summary>
        /// Alta transaccional de un consultorio. Devuelve el Id del nuevo tenant (DatosInstitucion.Id).
        /// Hace pre-chequeo de unicidad con SQL inline parametrizado; los unique indexes son la red final.
        /// </summary>
        public ModelResponse<int> RegistrarConsultorio(RegistroDTO d, string passwordHash)
        {
            var mr = new ModelResponse<int>();
            try
            {
                var rfc = (d.RFC ?? string.Empty).Trim().ToUpperInvariant();
                var nombre = (d.NombreComercial ?? string.Empty).Trim();
                var razonSocial = (d.RazonSocial ?? string.Empty).Trim();
                var email = (d.CorreoContacto ?? string.Empty).Trim();
                var userName = rfc;

                var duplicado = ObtenerMensajeDuplicado(rfc, nombre, razonSocial, email, userName);
                if (duplicado != null)
                {
                    mr.IsSuccess = false;
                    mr.Message = duplicado;
                    return mr;
                }

                BeginTransaction();
                try
                {
                    var nuevoId = InsertarDatosInstitucion(d);
                    if (nuevoId <= 0)
                    {
                        RollbackTransaction();
                        mr.IsSuccess = false;
                        mr.Message = "No fue posible registrar el consultorio.";
                        return mr;
                    }

                    var fiscalId = InsertarDatosFiscales(d, rfc, nuevoId);
                    if (fiscalId <= 0)
                    {
                        RollbackTransaction();
                        mr.IsSuccess = false;
                        mr.Message = "No fue posible registrar los datos fiscales del consultorio.";
                        return mr;
                    }

                    var usuarioId = InsertarUsuario(userName, passwordHash, nuevoId);
                    if (usuarioId <= 0)
                    {
                        RollbackTransaction();
                        mr.IsSuccess = false;
                        mr.Message = "No fue posible registrar el usuario administrador.";
                        return mr;
                    }

                    CommitTransaction();
                    mr.IsSuccess = true;
                    mr.Response = nuevoId;
                    mr.Message = "El consultorio se registró correctamente.";
                }
                catch (SqlException ex)
                {
                    RollbackTransaction();
                    Log.Error(ex, "Error de integridad al registrar consultorio");
                    mr.IsSuccess = false;
                    mr.Message = ObtenerMensajeDuplicadoSql(ex);
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    Log.Error(ex, "Error al registrar consultorio");
                    mr.IsSuccess = false;
                    mr.Message = "Ocurrió un error al registrar el consultorio.";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error general al registrar consultorio");
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al registrar el consultorio.";
            }
            return mr;
        }

        /// <summary>
        /// Pre-chequeo de unicidad global (inline parametrizado). Devuelve el mensaje del primer duplicado o null.
        /// </summary>
        private string ObtenerMensajeDuplicado(string rfc, string nombre, string razonSocial, string email, string userName)
        {
            if (Existe("SELECT COUNT(*) FROM dbo.DatosFiscales WHERE RFC = @RFC", new SqlParameter("@RFC", rfc)))
                return "El RFC ya se encuentra registrado.";

            if (Existe("SELECT COUNT(*) FROM dbo.DatosInstitucion WHERE Email = @Email", new SqlParameter("@Email", email)))
                return "El correo de contacto ya se encuentra registrado.";

            if (Existe("SELECT COUNT(*) FROM dbo.DatosInstitucion WHERE Nombre = @Nombre", new SqlParameter("@Nombre", nombre)))
                return "El nombre comercial ya se encuentra registrado.";

            if (Existe("SELECT COUNT(*) FROM dbo.DatosFiscales WHERE RazonSocial = @RazonSocial", new SqlParameter("@RazonSocial", razonSocial)))
                return "La razón social ya se encuentra registrada.";

            if (Existe("SELECT COUNT(*) FROM dbo.Usuario WHERE UserName = @UserName", new SqlParameter("@UserName", userName)))
                return "El nombre de usuario (RFC) ya se encuentra registrado.";

            return null;
        }

        private bool Existe(string sql, SqlParameter p)
        {
            var total = ExecuteScalar(sql, CommandType.Text, new[] { p });
            return total != null && Convert.ToInt32(total) > 0;
        }

        /// <summary>
        /// Traduce una excepción de restricción única (2601/2627) a un mensaje específico en español.
        /// </summary>
        private string ObtenerMensajeDuplicadoSql(SqlException ex)
        {
            if (ex.Number == 2601 || ex.Number == 2627)
            {
                var msg = ex.Message ?? string.Empty;
                if (msg.IndexOf("UX_DatosFiscales_RFC", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "El RFC ya se encuentra registrado.";
                if (msg.IndexOf("UX_DatosInstitucion_Email", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "El correo de contacto ya se encuentra registrado.";
                if (msg.IndexOf("UX_DatosInstitucion_Nombre", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "El nombre comercial ya se encuentra registrado.";
                if (msg.IndexOf("UX_DatosFiscales_RazonSocial", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "La razón social ya se encuentra registrada.";
                if (msg.IndexOf("UX_Usuario_UserName", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "El nombre de usuario (RFC) ya se encuentra registrado.";
                return "Los datos ya se encuentran registrados.";
            }
            return "Ocurrió un error al registrar el consultorio.";
        }

        /// <summary>Inserta DatosInstitucion (tenant) con Id=0. Devuelve el nuevo Id o 0.</summary>
        private int InsertarDatosInstitucion(RegistroDTO d)
        {
            var pars = new List<SqlParameter>
            {
                new SqlParameter("@Id", 0),
                new SqlParameter("@Nombre", (object)d.NombreComercial ?? DBNull.Value),
                new SqlParameter("@NoTrabajadores", DBNull.Value),
                new SqlParameter("@Telefono1", (object)d.Telefono ?? DBNull.Value),
                new SqlParameter("@Telefono2", DBNull.Value),
                new SqlParameter("@Email", (object)d.CorreoContacto ?? DBNull.Value),
                new SqlParameter("@Calle", (object)d.Calle ?? DBNull.Value),
                new SqlParameter("@NoInterior", (object)d.NoInterior ?? DBNull.Value),
                new SqlParameter("@NoExterior", (object)d.NoExterior ?? DBNull.Value),
                new SqlParameter("@Colonia", (object)d.Colonia ?? DBNull.Value),
                new SqlParameter("@Deleacion", DBNull.Value),
                new SqlParameter("@Municipio", (object)d.Municipio ?? DBNull.Value),
                new SqlParameter("@Estado", (object)d.Estado ?? DBNull.Value),
                new SqlParameter("@CP", (object)d.CP ?? DBNull.Value),
                new SqlParameter("@Pais", "México"),
                new SqlParameter("@MedicoNombre", (object)d.Responsable ?? DBNull.Value),
                new SqlParameter("@MedicoCedula", DBNull.Value),
                new SqlParameter("@MedicoEspecialidad", DBNull.Value)
            };

            var id = ExecuteScalar("SaveOrUpdateDatosInstitucion", CommandType.StoredProcedure, pars);
            return id == null ? 0 : Convert.ToInt32(id);
        }

        /// <summary>Inserta la fila fiscal 1:1 del tenant. Devuelve el nuevo Id o 0.</summary>
        private int InsertarDatosFiscales(RegistroDTO d, string rfc, int consultorioId)
        {
            var pars = new List<SqlParameter>
            {
                new SqlParameter("@Id", 0),
                new SqlParameter("@RazonSocial", (object)d.RazonSocial ?? DBNull.Value),
                new SqlParameter("@RFC", (object)rfc ?? DBNull.Value),
                new SqlParameter("@Telefono1", (object)d.Telefono ?? DBNull.Value),
                new SqlParameter("@Telefono2", DBNull.Value),
                new SqlParameter("@Email", (object)d.CorreoContacto ?? DBNull.Value),
                new SqlParameter("@Calle", (object)d.Calle ?? DBNull.Value),
                new SqlParameter("@NoInterior", (object)d.NoInterior ?? DBNull.Value),
                new SqlParameter("@NoExterior", (object)d.NoExterior ?? DBNull.Value),
                new SqlParameter("@Colonia", (object)d.Colonia ?? DBNull.Value),
                new SqlParameter("@Deleacion", DBNull.Value),
                new SqlParameter("@Municipio", (object)d.Municipio ?? DBNull.Value),
                new SqlParameter("@Estado", (object)d.Estado ?? DBNull.Value),
                new SqlParameter("@CP", (object)d.CP ?? DBNull.Value),
                new SqlParameter("@Pais", "México"),
                new SqlParameter("@ConsultorioID", consultorioId)
            };

            var id = ExecuteScalar("sp_nh_SaveDatosFiscal", CommandType.StoredProcedure, pars);
            return id == null ? 0 : Convert.ToInt32(id);
        }

        /// <summary>Inserta el usuario administrador (UserName = RFC normalizado). Devuelve el nuevo Id o 0.</summary>
        private int InsertarUsuario(string userName, string passwordHash, int companiaId)
        {
            var pars = new List<SqlParameter>
            {
                new SqlParameter("@Id", 0),
                new SqlParameter("@UserName", (object)userName ?? DBNull.Value),
                new SqlParameter("@Password", (object)passwordHash ?? DBNull.Value),
                new SqlParameter("@Estatus", true),
                new SqlParameter("@CompaniaID", companiaId)
            };

            var id = ExecuteScalar("sp_nh_SaveUsuario", CommandType.StoredProcedure, pars);
            return id == null ? 0 : Convert.ToInt32(id);
        }
    }
}
