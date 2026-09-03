using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.DAL;

namespace ConsultorioMedicoMVC.Services
{
    public class UsuarioService
    {
        private readonly HttpClientConnection _httpClient;

        public UsuarioService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<Usuario>>> ConsultarTodosUsuarios()
            => await _httpClient.ObtenerUsuarios();

        public async Task<Usuario> ObtenerUsuarioPorId(long id)
        {
            var r = await _httpClient.ObtenerUsuarioPorId(id);
            return (r.IsSuccess && r.Response != null) ? r.Response : null;
        }

        public async Task<ModelResponse<Usuario>> GuardarOActualizarUsuario(Usuario usuario)
        {
            var error = UsuarioValidator.Validar(usuario);
            if (error != null)
                return new ModelResponse<Usuario> { IsSuccess = false, Message = error };

            if (usuario.Id == 0)
                usuario.Estatus = true;

            return await _httpClient.GuardarOActualizarUsuario(usuario);
        }
    }
}
