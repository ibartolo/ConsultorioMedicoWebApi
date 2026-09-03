using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;
using Serilog;

namespace ConsultorioMedicoWebApi.DAL
{
    public partial class DbWrapper
    {
        private Usuario LlenarUsuario(IDataReader r)
        {
            return new Usuario
            {
                Id = Convert.ToInt32(r["Id"]),
                UserName = r["UserName"] as string,
                Password = r["Password"] as string,
                Estatus = Convert.ToBoolean(r["Estatus"]),
                CompaniaID = Convert.IsDBNull(r["CompaniaID"]) ? (int?)null : Convert.ToInt32(r["CompaniaID"])
            };
        }

        /// <summary>Lista usuarios. SP nuevo: sp_nh_GetAllUsuarios.</summary>
        public ModelResponse<List<Usuario>> ObtenerUsuarios(string usuario)
        {
            var mr = new ModelResponse<List<Usuario>>();
            try
            {
                var usuarios = GetObjects("sp_nh_GetAllUsuarios", LlenarUsuario, CommandType.StoredProcedure);
                mr.IsSuccess = true;
                mr.Response = usuarios.ToList();
                mr.Message = "Usuarios obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener usuarios para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los usuarios.";
            }
            return mr;
        }

        /// <summary>Obtiene un usuario por Id. SP nuevo: sp_nh_GetUsuarioById.</summary>
        public ModelResponse<Usuario> ObtenerUsuarioPorId(long id, string usuario)
        {
            var mr = new ModelResponse<Usuario>();
            try
            {
                var u = GetObject("sp_nh_GetUsuarioById", LlenarUsuario, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (u == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Usuario no encontrado.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = u;
                mr.Message = "Usuario obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener usuario {Id}", id);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el usuario.";
            }
            return mr;
        }

        /// <summary>Guarda o actualiza un usuario (upsert). SP nuevo: sp_nh_SaveUsuario. Devuelve el Id.</summary>
        public ModelResponse<Usuario> GuardarOActualizarUsuario(Usuario u, string usuario)
        {
            var mr = new ModelResponse<Usuario>();
            try
            {
                var pars = new List<SqlParameter>
                {
                    new SqlParameter("@Id", u.Id),
                    new SqlParameter("@UserName", (object)u.UserName ?? DBNull.Value),
                    new SqlParameter("@Password", (object)u.Password ?? DBNull.Value),
                    new SqlParameter("@Estatus", u.Estatus),
                    new SqlParameter("@CompaniaID", (object)u.CompaniaID ?? DBNull.Value)
                };

                var id = ExecuteScalar("sp_nh_SaveUsuario", CommandType.StoredProcedure, pars);
                if (id == null || Convert.ToInt64(id) == 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No fue posible guardar el usuario.";
                    return mr;
                }

                u.Id = Convert.ToInt32(id);
                mr.IsSuccess = true;
                mr.Response = u;
                mr.Message = "Usuario guardado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar usuario para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar el usuario.";
            }
            return mr;
        }
    }
}
