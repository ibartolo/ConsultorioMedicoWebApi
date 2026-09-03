using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
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

        public ModelResponse<List<Usuario>> ObtenerUsuarios(string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerUsuarios(usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerUsuarios");
                return new ModelResponse<List<Usuario>> { IsSuccess = false, Message = "Ocurrió un error al obtener los usuarios." };
            }
        }

        public ModelResponse<Usuario> ObtenerUsuarioPorId(long id, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerUsuarioPorId(id, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerUsuarioPorId");
                return new ModelResponse<Usuario> { IsSuccess = false, Message = "Ocurrió un error al obtener el usuario." };
            }
        }

        public ModelResponse<Usuario> GuardarOActualizarUsuario(Usuario usuario, string usuarioSesion)
        {
            try
            {
                var error = UsuarioValidator.Validar(usuario);
                if (error != null)
                    return new ModelResponse<Usuario> { IsSuccess = false, Message = error };

                if (usuario.Id == 0)
                    usuario.Estatus = true;

                return _dbWrapper.GuardarOActualizarUsuario(usuario, usuarioSesion);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarOActualizarUsuario");
                return new ModelResponse<Usuario> { IsSuccess = false, Message = "Ocurrió un error al guardar el usuario." };
            }
        }
    }
}
