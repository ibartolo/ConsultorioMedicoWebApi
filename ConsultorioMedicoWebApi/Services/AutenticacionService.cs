using System;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
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

                var usuario = _dbWrapper.AutenticarUsuario(userName, password);
                if (usuario == null)
                    return new ModelResponse<UsuarioDTO> { IsSuccess = false, Message = "Usuario o contraseña incorrectos." };

                var dto = new UsuarioDTO
                {
                    Id = usuario.Id,
                    UserName = usuario.UserName,
                    Password = usuario.Password,
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
