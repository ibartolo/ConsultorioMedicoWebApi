using System;
using System.Data;
using System.Data.SqlClient;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoWebApi.Helpers;
using Serilog;

namespace ConsultorioMedicoWebApi.DAL
{
    public partial class DbWrapper
    {
        /// <summary>
        /// Obtiene un usuario por UserName sin validar contraseña ni Estatus.
        /// SP: sp_nh_AutenticacionUsuario (solo @UserName; devuelve Password y Estatus
        /// para que la verificación se haga en C#).
        /// </summary>
        public Usuario ObtenerUsuarioPorUserName(string userName)
        {
            try
            {
                return GetObject("sp_nh_AutenticacionUsuario",
                    new Func<IDataReader, Usuario>(r => new Usuario
                    {
                        Id = Convert.ToInt32(r["Id"]),
                        UserName = r["UserName"] as string,
                        Password = r["Password"] as string,
                        Estatus = Convert.ToBoolean(r["Estatus"]),
                        CompaniaID = Convert.IsDBNull(r["CompaniaID"]) ? (int?)null : Convert.ToInt32(r["CompaniaID"])
                    }),
                    CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@UserName", (object)userName ?? DBNull.Value)
                    });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener usuario por UserName {UserName}", userName);
                return null;
            }
        }

        /// <summary>
        /// Actualiza únicamente la contraseña (hash PBKDF2) de un usuario.
        /// SP: sp_nh_UpdateUsuarioPassword. Se usa para el rehash transparente de legados.
        /// </summary>
        public void ActualizarPassword(int id, string passwordHash)
        {
            ExecuteNonQuery("sp_nh_UpdateUsuarioPassword", CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@Password", (object)passwordHash ?? DBNull.Value)
                });
        }

        /// <summary>
        /// Autentica al usuario verificando la contraseña en C# (PBKDF2 o texto plano legado).
        /// Devuelve el usuario SIN exponer el hash (Password = null) o null si el usuario no
        /// existe, está inactivo (Estatus = 0) o la contraseña es incorrecta.
        /// Si el password era legado (texto plano) y verifica, se rehashea a PBKDF2 de forma transparente.
        /// </summary>
        public Usuario AutenticarUsuario(string userName, string password)
        {
            var usuario = ObtenerUsuarioPorUserName(userName);
            if (usuario == null) return null;
            if (!usuario.Estatus) return null;

            if (!Cryptography.Verify(password, usuario.Password)) return null;

            if (!Cryptography.EsHashPbkdf2(usuario.Password))
                ActualizarPassword(usuario.Id, Cryptography.HashPassword(password));

            usuario.Password = null;
            return usuario;
        }
    }
}
