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
                Log.Information("AutenticacionService.AutenticarUsuario ENTRADA: {Json}", LogJson.ToJson(new { userName }));

                Log.Debug("AutenticacionService.AutenticarUsuario DB_REQUEST: {Json}", LogJson.ToJson(new { userName }));
                var usuario = _dbWrapper.ObtenerUsuarioPorUserName(userName);
                Log.Debug("AutenticacionService.AutenticarUsuario DB_RESPONSE: {Json}", LogJson.ToJson(new { usuario?.Id, usuario?.UserName, usuario?.Estatus, usuario?.CompaniaID }));

                if (usuario == null)
                {
                    Log.Warning("AutenticacionService.AutenticarUsuario VALIDACIÓN: {Mensaje}", "Usuario o contraseña incorrectos.");
                    return new ModelResponse<UsuarioDTO> { IsSuccess = false, Message = "Usuario o contraseña incorrectos." };
                }

                if (!usuario.Estatus)
                {
                    Log.Warning("AutenticacionService.AutenticarUsuario VALIDACIÓN: {Mensaje}", "El usuario se encuentra inactivo. Contacte al administrador del sistema.");
                    return new ModelResponse<UsuarioDTO> { IsSuccess = false, Message = "El usuario se encuentra inactivo. Contacte al administrador del sistema." };
                }

                if (!Cryptography.Verify(password, usuario.Password))
                {
                    Log.Warning("AutenticacionService.AutenticarUsuario VALIDACIÓN: {Mensaje}", "Usuario o contraseña incorrectos.");
                    return new ModelResponse<UsuarioDTO> { IsSuccess = false, Message = "Usuario o contraseña incorrectos." };
                }

                // Rehash transparente: si era texto plano legado y verificó, se persiste el hash PBKDF2.
                if (!Cryptography.EsHashPbkdf2(usuario.Password))
                {
                    Log.Debug("AutenticacionService.AutenticarUsuario DB_REQUEST (rehash): {Json}", LogJson.ToJson(new { usuario.Id }));
                    _dbWrapper.ActualizarPassword(usuario.Id, Cryptography.HashPassword(password));
                }

                var dto = new UsuarioDTO
                {
                    Id = usuario.Id,
                    UserName = usuario.UserName,
                    Password = null,
                    Email = usuario.Email,
                    Estatus = usuario.Estatus,
                    CompaniaID = usuario.CompaniaID,
                    FechaVigenciaFin = usuario.FechaVigenciaFin,
                    EsPeriodoPrueba = usuario.EsPeriodoPrueba,
                    Plan = usuario.Plan,
                    LicenciaEstatus = usuario.LicenciaEstatus
                };

                Log.Information("AutenticacionService.AutenticarUsuario RESULTADO: IsSuccess=True, Message=Usuario autenticado correctamente., Id={Id}", dto.Id);
                return new ModelResponse<UsuarioDTO>
                {
                    IsSuccess = true,
                    Response = dto,
                    Message = "Usuario autenticado correctamente."
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AutenticacionService.AutenticarUsuario ERROR: {Message}", ex.Message);
                return new ModelResponse<UsuarioDTO> { IsSuccess = false, Message = "Ocurrió un error al autenticar el usuario." };
            }
        }
    }
}
