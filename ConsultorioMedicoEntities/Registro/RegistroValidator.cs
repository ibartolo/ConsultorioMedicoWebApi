using System.Text.RegularExpressions;

namespace ConsultorioMedicoEntities.Registro
{
    /// <summary>
    /// Validaciones del registro público de consultorio, compartidas entre MVC y WebApi.
    /// Devuelve el primer error encontrado (en español) o null si no hay errores.
    /// No valida unicidad en BD (eso es responsabilidad de la capa de datos).
    /// </summary>
    public static class RegistroValidator
    {
        private const string PatronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        private const string PatronRfc = @"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$";
        // Letras (incl. acentos/ñ), números, espacios y signos de negocio comunes.
        private const string PatronNombreComercial = @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9\s.,&'()/\-]+$";
        private const string PatronResponsable = @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s.]+$";
        private const string PatronTelefono = @"^[0-9+\-()\s.]+$";

        public static string Validar(RegistroDTO r)
        {
            if (r == null) return "Los datos de registro son inválidos.";

            if (string.IsNullOrWhiteSpace(r.NombreComercial)) return "El nombre comercial es requerido.";
            if (string.IsNullOrWhiteSpace(r.RazonSocial)) return "La razón social es requerida.";
            if (string.IsNullOrWhiteSpace(r.RFC)) return "El RFC es requerido.";
            if (string.IsNullOrWhiteSpace(r.Responsable)) return "El nombre del responsable es requerido.";
            if (string.IsNullOrWhiteSpace(r.Calle)) return "La calle es requerida.";
            if (string.IsNullOrWhiteSpace(r.CorreoContacto)) return "El correo de contacto es requerido.";
            if (string.IsNullOrEmpty(r.Password)) return "La contraseña es requerida.";
            if (string.IsNullOrEmpty(r.Confirmacion)) return "La confirmación de la contraseña es requerida.";
            if (!string.Equals(r.Password, r.Confirmacion, System.StringComparison.Ordinal))
                return "Las contraseñas no coinciden.";

            // Política mínima server-side (alineada con minlength:6 del cliente). Sin complejidad adicional.
            // La contraseña NO se recorta; solo se rechazan las vacías, cortas o de solo espacios.
            if (r.Password.Length < 6) return "La contraseña debe tener al menos 6 caracteres.";
            if (string.IsNullOrWhiteSpace(r.Password)) return "La contraseña no puede contener solo espacios.";

            // RFC normalizado (trim + mayúsculas) con el patrón de HelpDesk (12-13 posiciones).
            var rfc = (r.RFC ?? string.Empty).Trim().ToUpper();
            if (!Regex.IsMatch(rfc, PatronRfc)) return "El RFC no tiene un formato válido.";

            if (!Regex.IsMatch(r.CorreoContacto, PatronEmail)) return "Ingrese un formato de correo válido.";

            if (r.CP.HasValue && (r.CP.Value < 1 || r.CP.Value > 99999))
                return "El código postal debe ser un número entre 1 y 99999.";

            if (!string.IsNullOrWhiteSpace(r.Telefono) && !Regex.IsMatch(r.Telefono.Trim(), PatronTelefono))
                return "El teléfono contiene caracteres no válidos.";

            if (!Regex.IsMatch(r.NombreComercial, PatronNombreComercial))
                return "El nombre comercial contiene caracteres no válidos.";

            if (!Regex.IsMatch(r.RazonSocial, PatronNombreComercial))
                return "La razón social contiene caracteres no válidos.";

            if (!Regex.IsMatch(r.Responsable, PatronResponsable))
                return "El nombre del responsable solo puede contener letras y espacios.";

            return null;
        }
    }
}
