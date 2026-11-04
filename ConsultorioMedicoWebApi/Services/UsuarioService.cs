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
                return _dbWrapper.ObtenerUsuarios(usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerUsuarios");
                return new ModelResponse<List<Usuario>> { IsSuccess = false, Message = "Ocurrió un error al obtener los usuarios." };
            }
        }

        public ModelResponse<Usuario> ObtenerUsuarioPorId(long id, string usuario, int empresaId)
        {
            try
            {
                return _dbWrapper.ObtenerUsuarioPorId(id, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerUsuarioPorId");
                return new ModelResponse<Usuario> { IsSuccess = false, Message = "Ocurrió un error al obtener el usuario." };
            }
        }

        public ModelResponse<Usuario> GuardarOActualizarUsuario(Usuario usuario, string usuarioSesion, int empresaId)
        {
            try
            {
                var error = UsuarioValidator.Validar(usuario);
                if (error != null)
                    return new ModelResponse<Usuario> { IsSuccess = false, Message = error };

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

                return _dbWrapper.GuardarOActualizarUsuario(usuario, usuarioSesion, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarOActualizarUsuario");
                return new ModelResponse<Usuario> { IsSuccess = false, Message = "Ocurrió un error al guardar el usuario." };
            }
        }
    }
}
