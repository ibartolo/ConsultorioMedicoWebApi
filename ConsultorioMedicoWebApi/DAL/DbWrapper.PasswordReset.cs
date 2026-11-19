using System;
using System.Data;
using System.Data.SqlClient;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Recuperacion;
using Serilog;

namespace ConsultorioMedicoWebApi.DAL
{
    /// <summary>
    /// Recuperación de contraseña. Acceso a los SPs del flujo de recuperación
    /// (sp_nh_GetUsuarioByEmail, sp_nh_CrearTokenRecuperacion, sp_nh_GetTokenRecuperacion,
    /// sp_nh_RestablecerPasswordConToken). El restablecimiento es atómico en el SP.
    /// </summary>
    public partial class DbWrapper
    {
        /// <summary>
        /// Busca un usuario activo por Email. SP: sp_nh_GetUsuarioByEmail (filtra Estatus = 1).
        /// Devuelve null si no existe o está inactivo (no revela la existencia).
        /// </summary>
        public Usuario ObtenerUsuarioPorEmail(string email)
        {
            try
            {
                return GetObject("sp_nh_GetUsuarioByEmail", LlenarUsuario, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Email", (object)email ?? DBNull.Value) });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener usuario por Email {Email}", email);
                return null;
            }
        }

        /// <summary>
        /// Invalida los tokens no usados del usuario y crea uno nuevo.
        /// SP: sp_nh_CrearTokenRecuperacion. Devuelve el Id del nuevo token o 0.
        /// </summary>
        public int CrearTokenRecuperacion(int usuarioId, Guid token, int minutos = 10)
        {
            var id = ExecuteScalar("sp_nh_CrearTokenRecuperacion", CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("@UsuarioId", usuarioId),
                    new SqlParameter("@Token", token),
                    new SqlParameter("@Minutos", minutos)
                });

            return id == null || id == DBNull.Value ? 0 : Convert.ToInt32(id);
        }

        /// <summary>
        /// Obtiene un token de recuperación con los datos del usuario asociado.
        /// SP: sp_nh_GetTokenRecuperacion. Devuelve null si el token no existe.
        /// </summary>
        public RecuperacionTokenDTO ObtenerTokenRecuperacion(Guid token)
        {
            try
            {
                return GetObject("sp_nh_GetTokenRecuperacion",
                    new Func<IDataReader, RecuperacionTokenDTO>(r => new RecuperacionTokenDTO
                    {
                        TokenId = Convert.ToInt32(r["TokenId"]),
                        UsuarioId = Convert.ToInt32(r["UsuarioId"]),
                        UserName = r["UserName"] as string,
                        Email = r["Email"] as string,
                        FechaCreacion = Convert.ToDateTime(r["FechaCreacion"]),
                        FechaExpiracion = Convert.ToDateTime(r["FechaExpiracion"]),
                        Usado = Convert.ToBoolean(r["Usado"]),
                        FechaUso = Convert.IsDBNull(r["FechaUso"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaUso"]),
                        Vigente = Convert.ToBoolean(r["Vigente"])
                    }),
                    CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Token", token) });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener token de recuperación {Token}", token);
                return null;
            }
        }

        /// <summary>
        /// Valida el token (no usado y no expirado) y actualiza la contraseña de forma atómica,
        /// marcando el token como usado. SP: sp_nh_RestablecerPasswordConToken.
        /// Devuelve 1 si se restableció, 0 si el token es inválido/expirado/usado.
        /// </summary>
        public int RestablecerPasswordConToken(Guid token, string passwordHash)
        {
            var resultado = ExecuteScalar("sp_nh_RestablecerPasswordConToken", CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("@Token", token),
                    new SqlParameter("@Password", (object)passwordHash ?? DBNull.Value)
                });

            return resultado == null || resultado == DBNull.Value ? 0 : Convert.ToInt32(resultado);
        }
    }
}
