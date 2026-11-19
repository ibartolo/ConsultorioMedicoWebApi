using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using ConsultorioMedicoEntities.Registro;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    /// <summary>
    /// Servicio del registro público de consultorio. Valida, normaliza el RFC, hashea la contraseña
    /// (PBKDF2) y delega el alta transaccional en DbWrapper.Registro. Nunca devuelve contraseña/hash.
    /// </summary>
    public class RegistroService
    {
        private readonly DbWrapper _dbWrapper;

        public RegistroService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse RegistrarConsultorio(RegistroDTO datos)
        {
            try
            {
                // Normaliza los campos de captura (trim) antes de validar y de insertar.
                // El RFC se normaliza aparte (trim + mayúsculas); la contraseña NO se recorta.
                Normalizar(datos);

                var error = RegistroValidator.Validar(datos);
                if (error != null)
                    return new ModelResponse { IsSuccess = false, Message = error };

                // RFC normalizado (trim + mayúsculas) como UserName del primer admin.
                datos.RFC = (datos.RFC ?? string.Empty).Trim().ToUpperInvariant();

                var passwordHash = Cryptography.HashPassword(datos.Password);

                var resultado = _dbWrapper.RegistrarConsultorio(datos, passwordHash);

                // Correo de bienvenida best-effort: si el SMTP falla solo se registra el error;
                // el consultorio ya quedó dado de alta y NO se revierte.
                if (resultado.IsSuccess)
                    EnviarCorreoBienvenida(datos);

                return new ModelResponse
                {
                    IsSuccess = resultado.IsSuccess,
                    Message = resultado.Message
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en RegistrarConsultorio");
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al registrar el consultorio." };
            }
        }

        /// <summary>
        /// Envía el correo de bienvenida tras dar de alta el consultorio. Es best-effort:
        /// si el envío falla solo se registra el error (el alta no se revierte).
        /// Por seguridad, el correo NO incluye la contraseña elegida por el usuario.
        /// </summary>
        private static void EnviarCorreoBienvenida(RegistroDTO datos)
        {
            try
            {
                var ruta = HostingEnvironment.MapPath("~/Template/Template_AltaConsultorio.html");
                if (string.IsNullOrEmpty(ruta) || !File.Exists(ruta))
                {
                    Log.Warning("Plantilla de correo de bienvenida no encontrada en {Ruta}", ruta);
                    return;
                }

                var plantilla = File.ReadAllText(ruta);

                var baseUri = ConfigurationManager.AppSettings["BaseUri"] ?? string.Empty;
                if (!baseUri.EndsWith("/"))
                    baseUri += "/";

                var cuerpo = plantilla
                    .Replace("{{NombreComercial}}", datos.NombreComercial ?? string.Empty)
                    .Replace("{{Responsable}}", datos.Responsable ?? string.Empty)
                    .Replace("{{Usuario}}", datos.RFC ?? string.Empty)
                    .Replace("{{UrlLogin}}", baseUri + "Home/Autentication")
                    .Replace("{{UrlTerminos}}", baseUri + "Home/Terminos")
                    .Replace("{{UrlPrivacidad}}", baseUri + "Home/Privacidad");

                EmailHelper.EnviarEmail(
                    new List<string> { datos.CorreoContacto },
                    "Bienvenido a Consultorio Médico - Tu acceso",
                    cuerpo);
            }
            catch (Exception ex)
            {
                // Best-effort: el consultorio ya quedó registrado; solo se informa del fallo.
                Log.Error(ex, "No se pudo enviar el correo de bienvenida a {Correo}", datos.CorreoContacto);
            }
        }

        /// <summary>
        /// Recorta los espacios iniciales/finales de los campos de captura para que el valor
        /// persistido sea consistente con el usado en el pre-chequeo de unicidad (que ya recorta).
        /// La contraseña y la confirmación NO se recortan (solo se validan en RegistroValidator).
        /// </summary>
        private static void Normalizar(RegistroDTO d)
        {
            if (d == null) return;

            d.NombreComercial = Trim(d.NombreComercial);
            d.RazonSocial = Trim(d.RazonSocial);
            d.Responsable = Trim(d.Responsable);
            d.Calle = Trim(d.Calle);
            d.NoExterior = Trim(d.NoExterior);
            d.NoInterior = Trim(d.NoInterior);
            d.Colonia = Trim(d.Colonia);
            d.Municipio = Trim(d.Municipio);
            d.Estado = Trim(d.Estado);
            d.Telefono = Trim(d.Telefono);
            d.CorreoContacto = Trim(d.CorreoContacto);
        }

        private static string Trim(string valor)
        {
            return valor == null ? null : valor.Trim();
        }
    }
}
