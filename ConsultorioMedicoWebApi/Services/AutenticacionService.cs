using System;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class AutenticacionService
    {
        private readonly DbWrapper _dbWrapper;

        public AutenticacionService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<UsuarioDTO> AutenticarUsuario(string userName, string password)
        {
            try
            {
                Log.Information("AutenticacionService.AutenticarUsuario para {UserName}", userName);

                var usuario = _dbWrapper.ObtenerUsuarioPorUserName(userName);
                if (usuario == null)
                    return new ModelResponse<UsuarioDTO> { IsSuccess = false, Message = "Usuario o contraseña incorrectos." };

                if (!usuario.Estatus)
                    return new ModelResponse<UsuarioDTO> { IsSuccess = false, Message = "El usuario se encuentra inactivo. Contacte al administrador del sistema." };

                if (!Cryptography.Verify(password, usuario.Password))
                    return new ModelResponse<UsuarioDTO> { IsSuccess = false, Message = "Usuario o contraseña incorrectos." };

                // Rehash transparente: si era texto plano legado y verificó, se persiste el hash PBKDF2.
                if (!Cryptography.EsHashPbkdf2(usuario.Password))
                    _dbWrapper.ActualizarPassword(usuario.Id, Cryptography.HashPassword(password));

                var dto = new UsuarioDTO
                {
                    Id = usuario.Id,
                    UserName = usuario.UserName,
                    Password = null,
                    Estatus = usuario.Estatus,
                    CompaniaID = usuario.CompaniaID
                };

                return new ModelResponse<UsuarioDTO>
                {
                    IsSuccess = true,
                    Response = dto,
                    Message = "Usuario autenticado correctamente."
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticarUsuario");
                return new ModelResponse<UsuarioDTO> { IsSuccess = false, Message = "Ocurrió un error al autenticar el usuario." };
            }
        }
    }
}
