using System;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Recuperacion;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        /// <summary>
        /// Solicita la recuperación de contraseña por correo. Endpoint anónimo: no requiere token
        /// (se pasa null para que HttpClientBase no agregue el encabezado Authorization).
        /// </summary>
        public async Task<ModelResponse<object>> SolicitarRecuperacion(string email)
            => await RequestAsync<object>("api/RecuperacionPassword/Solicitar", HttpMethod.Post,
                new SolicitarRecuperacionDTO { Email = email }, null);

        /// <summary>
        /// Valida un token de recuperación. Endpoint anónimo (token null). El token viaja
        /// URL-encoded en el query string.
        /// </summary>
        public async Task<ModelResponse<object>> ValidarTokenRecuperacion(string token)
            => await RequestAsync<object>("api/RecuperacionPassword/Validar?token=" + Uri.EscapeDataString(token ?? string.Empty),
                HttpMethod.Get, null, null);

        /// <summary>
        /// Restablece la contraseña usando el token de recuperación. Endpoint anónimo (token null).
        /// </summary>
        public async Task<ModelResponse<object>> RestablecerPassword(string token, string password, string confirmacion)
            => await RequestAsync<object>("api/RecuperacionPassword/Restablecer", HttpMethod.Post,
                new RestablecerPasswordDTO { Token = token, Password = password, Confirmacion = confirmacion }, null);
    }
}
