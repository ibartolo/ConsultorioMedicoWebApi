using Newtonsoft.Json;
using System;

namespace ConsultorioMedicoWebApi.Helpers
{
    /// <summary>
    /// Serializa objetos a JSON compacto (una línea) para logging en los servicios WebApi.
    /// NO enmascara datos de negocio (a diferencia de LogSanitizer del HelpDesk); el llamador
    /// es responsable de NO incluir credenciales (Password, Contrasena, Confirmacion, tokens)
    /// ni contenido binario en los objetos que serializa.
    /// </summary>
    public static class LogJson
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None
        };

        /// <summary>Serializa un objeto a JSON compacto. Maneja null y errores sin lanzar excepciones.</summary>
        public static string ToJson(object obj)
        {
            if (obj == null) return "null";
            try
            {
                return JsonConvert.SerializeObject(obj, Settings);
            }
            catch (Exception ex)
            {
                return "<no serializable: " + ex.Message + ">";
            }
        }
    }
}
