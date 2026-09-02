using System;
using System.Data;
using System.Data.SqlClient;
using ConsultorioMedicoEntities.Autenticacion;
using Serilog;

namespace ConsultorioMedicoWebApi.DAL
{
    public partial class DbWrapper
    {
        /// <summary>
        /// Autentica al usuario comparando UserName y Password en texto plano,
        /// igual que la aplicación WinForms (NHibernate CreateCriteria).
        /// NO valida Estatus (fidelidad con el comportamiento actual).
        /// SP: sp_nh_AutenticacionUsuario.
        /// </summary>
        public Usuario AutenticarUsuario(string userName, string password)
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
                        new SqlParameter("@UserName", (object)userName ?? DBNull.Value),
                        new SqlParameter("@Password", (object)password ?? DBNull.Value)
                    });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al autenticar usuario {UserName}", userName);
                return null;
            }
        }
    }
}
