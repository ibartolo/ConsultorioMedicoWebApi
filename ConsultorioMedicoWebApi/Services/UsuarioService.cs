using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class UsuarioService
    {
        private readonly DbWrapper _dbWrapper;

        public UsuarioService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Usuario>> ObtenerUsuarios(string usuario, int empresaId)
        {
            try
            {
                Log.Information("UsuarioService.ObtenerUsuarios ENTRADA: {Json}", LogJson.ToJson(new { usuario, empresaId }));

                Log.Debug("UsuarioService.ObtenerUsuarios DB_REQUEST: {Json}", LogJson.ToJson(new { usuario, empresaId }));
                var result = _dbWrapper.ObtenerUsuarios(usuario, empresaId);
                Log.Debug("UsuarioService.ObtenerUsuarios DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("UsuarioService.ObtenerUsuarios RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UsuarioService.ObtenerUsuarios ERROR: {Message}", ex.Message);
                return new ModelResponse<List<Usuario>> { IsSuccess = false, Message = "Ocurrió un error al obtener los usuarios." };
            }
        }

        public ModelResponse<Usuario> ObtenerUsuarioPorId(long id, string usuario, int empresaId)
        {
            try
            {
                Log.Information("UsuarioService.ObtenerUsuarioPorId ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                Log.Debug("UsuarioService.ObtenerUsuarioPorId DB_REQUEST: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var result = _dbWrapper.ObtenerUsuarioPorId(id, usuario, empresaId);
                Log.Debug("UsuarioService.ObtenerUsuarioPorId DB_RESPONSE: {Json}", LogJson.ToJson(new { result?.Response?.Id, result?.Response?.UserName, result?.Response?.Email, result?.Response?.Estatus, result?.Response?.CompaniaID, result?.IsSuccess, result?.Message }));

                Log.Information("UsuarioService.ObtenerUsuarioPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UsuarioService.ObtenerUsuarioPorId ERROR: {Message}", ex.Message);
                return new ModelResponse<Usuario> { IsSuccess = false, Message = "Ocurrió un error al obtener el usuario." };
            }
        }

        public ModelResponse<Usuario> GuardarOActualizarUsuario(Usuario usuario, string usuarioSesion, int empresaId)
        {
            try
            {
                Log.Information("UsuarioService.GuardarOActualizarUsuario ENTRADA: {Json}", LogJson.ToJson(new { usuario.Id, usuario.UserName, usuario.Email, usuario.Estatus, usuario.CompaniaID, usuarioSesion, empresaId }));

                var error = UsuarioValidator.Validar(usuario);
                if (error != null)
                {
                    Log.Warning("UsuarioService.GuardarOActualizarUsuario VALIDACIÓN: {Mensaje}", error);
                    return new ModelResponse<Usuario> { IsSuccess = false, Message = error };
                }

                if (usuario.Id == 0)
                {
                    usuario.Estatus = true;
                    usuario.Password = Cryptography.HashPassword(usuario.Password);
                }
                else if (!string.IsNullOrWhiteSpace(usuario.Password))
                {
                    usuario.Password = Cryptography.HashPassword(usuario.Password);
                }
                else
                {
                    // Edición con contraseña vacía: se pasa NULL para que el SP
                    // conserve el hash actual (Password = ISNULL(@Password, Password)).
                    usuario.Password = null;
                }

                // El consultorio (CompaniaID) SIEMPRE se asigna desde el claim; el cliente no elige consultorio.
                usuario.CompaniaID = empresaId;

                Log.Debug("UsuarioService.GuardarOActualizarUsuario DB_REQUEST: {Json}", LogJson.ToJson(new { usuario.Id, usuario.UserName, usuario.Email, usuario.Estatus, usuario.CompaniaID, usuarioSesion, empresaId }));
                var result = _dbWrapper.GuardarOActualizarUsuario(usuario, usuarioSesion, empresaId);
                Log.Debug("UsuarioService.GuardarOActualizarUsuario DB_RESPONSE: {Json}", LogJson.ToJson(new { result?.Response?.Id, result?.Response?.UserName, result?.Response?.Email, result?.Response?.Estatus, result?.Response?.CompaniaID, result?.IsSuccess, result?.Message }));

                Log.Information("UsuarioService.GuardarOActualizarUsuario RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UsuarioService.GuardarOActualizarUsuario ERROR: {Message}", ex.Message);
                return new ModelResponse<Usuario> { IsSuccess = false, Message = "Ocurrió un error al guardar el usuario." };
            }
        }
    }
}
