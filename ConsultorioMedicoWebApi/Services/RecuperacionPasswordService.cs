using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    /// <summary>
    /// Servicio del flujo de recuperación de contraseña por correo.
    /// - Solicitar: genera un token de un solo uso (10 min) y envía un correo con un enlace.
    ///   Nunca revela si el correo existe o no (respuesta genérica para correo desconocido/inactivo/formato inválido).
    /// - Validar: comprueba que el token exista, no esté usado y no haya expirado.
    /// - Restablecer: valida la nueva contraseña y la actualiza de forma atómica (marca el token como usado).
    /// </summary>
    public class RecuperacionPasswordService
    {
        private const string MensajeEnviado = "Se envió satisfactoriamente.";
        private const string MensajeErrorEnvio = "No fue posible enviar el correo. Intente nuevamente.";
        private const string MensajeEnlaceInvalido = "El enlace es inválido o ha expirado.";
        private const string MensajeActualizado = "Contraseña actualizada correctamente.";
        private const string MensajePasswordCorta = "La contraseña debe tener al menos 6 caracteres.";
        private const string MensajeConfirmacion = "La confirmación de la contraseña no coincide.";
        private const int MinutosExpiracion = 10;

        private readonly DbWrapper _dbWrapper;

        public RecuperacionPasswordService()
        {
            _dbWrapper = new DbWrapper();
        }

        /// <summary>
        /// Solicita la recuperación de contraseña por correo. Devuelve siempre "Se envió satisfactoriamente."
        /// salvo error real (BD, plantilla faltante o SMTP). Nunca revela si el correo existe.
        /// </summary>
        public ModelResponse Solicitar(string email)
        {
            try
            {
                Log.Information("RecuperacionPasswordService.Solicitar ENTRADA: {Json}", LogJson.ToJson(new { email }));

                var correo = (email ?? string.Empty).Trim();

                // Correo vacío o con formato inválido: respuesta genérica, sin enviar.
                if (string.IsNullOrEmpty(correo) || !EsEmailValido(correo))
                {
                    Log.Information("RecuperacionPasswordService.Solicitar RESULTADO: IsSuccess=True, Message={Message}", MensajeEnviado);
                    return new ModelResponse { IsSuccess = true, Message = MensajeEnviado };
                }

                // Usuario activo con ese correo. Si no existe o está inactivo: respuesta genérica, sin enviar.
                Log.Debug("RecuperacionPasswordService.Solicitar DB_REQUEST: {Json}", LogJson.ToJson(new { correo }));
                var usuario = _dbWrapper.ObtenerUsuarioPorEmail(correo);
                Log.Debug("RecuperacionPasswordService.Solicitar DB_RESPONSE: {Json}", LogJson.ToJson(new { usuario?.Id, usuario?.UserName, usuario?.Email, usuario?.Estatus }));

                if (usuario == null)
                {
                    Log.Information("RecuperacionPasswordService.Solicitar RESULTADO: IsSuccess=True, Message={Message}", MensajeEnviado);
                    return new ModelResponse { IsSuccess = true, Message = MensajeEnviado };
                }

                // Token de un solo uso con expiración de 10 minutos. El SP calcula la
                // expiración con GETDATE() e invalida los tokens no usados previos.
                var token = Guid.NewGuid();

                Log.Debug("RecuperacionPasswordService.Solicitar DB_REQUEST (crear token): {Json}", LogJson.ToJson(new { usuario.Id, MinutosExpiracion }));
                _dbWrapper.CrearTokenRecuperacion(usuario.Id, token, MinutosExpiracion);

                EnviarCorreoRecuperacion(usuario.UserName, usuario.Email, token);

                Log.Information("RecuperacionPasswordService.Solicitar RESULTADO: IsSuccess=True, Message={Message}", MensajeEnviado);
                return new ModelResponse { IsSuccess = true, Message = MensajeEnviado };
            }
            catch (Exception ex)
            {
                // Error real (BD, plantilla faltante o SMTP): se informa el fallo.
                Log.Error(ex, "RecuperacionPasswordService.Solicitar ERROR: {Message}", ex.Message);
                return new ModelResponse { IsSuccess = false, Message = MensajeErrorEnvio };
            }
        }

        /// <summary>
        /// Valida un token de recuperación: debe existir, no estar usado y no haber expirado.
        /// </summary>
        public ModelResponse Validar(string token)
        {
            try
            {
                Log.Information("RecuperacionPasswordService.Validar ENTRADA: {Json}", LogJson.ToJson(new { TokenProporcionado = !string.IsNullOrWhiteSpace(token) }));

                Guid tokenGuid;
                if (!Guid.TryParse(token, out tokenGuid))
                {
                    Log.Warning("RecuperacionPasswordService.Validar VALIDACIÓN: {Mensaje}", MensajeEnlaceInvalido);
                    return new ModelResponse { IsSuccess = false, Message = MensajeEnlaceInvalido };
                }

                Log.Debug("RecuperacionPasswordService.Validar DB_REQUEST: {Json}", LogJson.ToJson(new { TokenProporcionado = true }));
                var registro = _dbWrapper.ObtenerTokenRecuperacion(tokenGuid);
                Log.Debug("RecuperacionPasswordService.Validar DB_RESPONSE: {Json}", LogJson.ToJson(new { registro?.UsuarioId, registro?.UserName, registro?.Email, registro?.Vigente }));

                if (registro == null || !registro.Vigente)
                {
                    Log.Warning("RecuperacionPasswordService.Validar VALIDACIÓN: {Mensaje}", MensajeEnlaceInvalido);
                    return new ModelResponse { IsSuccess = false, Message = MensajeEnlaceInvalido };
                }

                Log.Information("RecuperacionPasswordService.Validar RESULTADO: IsSuccess=True, Message=El enlace es válido.");
                return new ModelResponse { IsSuccess = true, Message = "El enlace es válido." };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "RecuperacionPasswordService.Validar ERROR: {Message}", ex.Message);
                return new ModelResponse { IsSuccess = false, Message = MensajeEnlaceInvalido };
            }
        }

        /// <summary>
        /// Restablece la contraseña con un token de recuperación. Valida la nueva contraseña
        /// (mínimo 6 caracteres y confirmación coincidente), la hashea (PBKDF2) y actualiza al
        /// usuario de forma atómica marcando el token como usado.
        /// </summary>
        public ModelResponse Restablecer(string token, string password, string confirmacion)
        {
            try
            {
                Log.Information("RecuperacionPasswordService.Restablecer ENTRADA: {Json}", LogJson.ToJson(new { TokenProporcionado = !string.IsNullOrWhiteSpace(token), PasswordProporcionada = !string.IsNullOrWhiteSpace(password), ConfirmacionProporcionada = !string.IsNullOrWhiteSpace(confirmacion) }));

                Guid tokenGuid;
                if (!Guid.TryParse(token, out tokenGuid))
                {
                    Log.Warning("RecuperacionPasswordService.Restablecer VALIDACIÓN: {Mensaje}", MensajeEnlaceInvalido);
                    return new ModelResponse { IsSuccess = false, Message = MensajeEnlaceInvalido };
                }

                var passwordError = PasswordPolicy.Validar(password);
                if (passwordError != null)
                {
                    Log.Warning("RecuperacionPasswordService.Restablecer VALIDACIÓN: {Mensaje}", passwordError);
                    return new ModelResponse { IsSuccess = false, Message = passwordError };
                }

                if (!string.Equals(password, confirmacion, StringComparison.Ordinal))
                {
                    Log.Warning("RecuperacionPasswordService.Restablecer VALIDACIÓN: {Mensaje}", MensajeConfirmacion);
                    return new ModelResponse { IsSuccess = false, Message = MensajeConfirmacion };
                }

                var passwordHash = Cryptography.HashPassword(password);

                Log.Debug("RecuperacionPasswordService.Restablecer DB_REQUEST: {Json}", LogJson.ToJson(new { TokenProporcionado = true }));
                var resultado = _dbWrapper.RestablecerPasswordConToken(tokenGuid, passwordHash);
                Log.Debug("RecuperacionPasswordService.Restablecer DB_RESPONSE: {Json}", LogJson.ToJson(resultado));

                if (resultado == 0)
                {
                    Log.Warning("RecuperacionPasswordService.Restablecer VALIDACIÓN: {Mensaje}", MensajeEnlaceInvalido);
                    return new ModelResponse { IsSuccess = false, Message = MensajeEnlaceInvalido };
                }

                Log.Information("RecuperacionPasswordService.Restablecer RESULTADO: IsSuccess=True, Message={Message}", MensajeActualizado);
                return new ModelResponse { IsSuccess = true, Message = MensajeActualizado };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "RecuperacionPasswordService.Restablecer ERROR: {Message}", ex.Message);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al restablecer la contraseña." };
            }
        }

        /// <summary>
        /// Lee la plantilla del correo, construye el enlace de restablecimiento y envía el correo.
        /// Si la plantilla falta o el SMTP falla, propaga la excepción (se trata como error real).
        /// Por seguridad, el correo NO incluye la contraseña.
        /// </summary>
        private static void EnviarCorreoRecuperacion(string userName, string email, Guid token)
        {
            var ruta = HostingEnvironment.MapPath("~/Template/Template_RecuperacionPassword.html");
            if (string.IsNullOrEmpty(ruta) || !File.Exists(ruta))
                throw new FileNotFoundException("Plantilla de recuperación de contraseña no encontrada.", ruta);

            var plantilla = File.ReadAllText(ruta);

            var baseUri = ConfigurationManager.AppSettings["BaseUri"] ?? string.Empty;
            if (!baseUri.EndsWith("/"))
                baseUri += "/";

            var urlReset = baseUri + "RecuperacionPassword/Restablecer?token=" + token;

            var cuerpo = plantilla
                .Replace("{{Usuario}}", userName ?? string.Empty)
                .Replace("{{UrlReset}}", urlReset)
                .Replace("{{Minutos}}", MinutosExpiracion.ToString())
                .Replace("{{UrlTerminos}}", baseUri + "Home/Terminos")
                .Replace("{{UrlPrivacidad}}", baseUri + "Home/Privacidad");

            EmailHelper.EnviarEmail(
                new List<string> { email },
                "Restablecer contraseña - Consultorio Médico",
                cuerpo);
        }

        private static bool EsEmailValido(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
