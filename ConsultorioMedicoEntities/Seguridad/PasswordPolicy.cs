using System.Text.RegularExpressions;

namespace ConsultorioMedicoEntities.Seguridad
{
    /// <summary>
    /// Política mínima de contraseña para el flujo de recuperación:
    /// mínimo 6 caracteres, al menos una mayúscula, una minúscula, un número y un símbolo.
    /// El símbolo es cualquier carácter no alfanumérico que no sea espacio.
    /// </summary>
    public static class PasswordPolicy
    {
        public static string Validar(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "La contraseña es requerida.";

            if (password.Length < 6)
                return "La contraseña debe tener al menos 6 caracteres.";

            if (!Regex.IsMatch(password, "[A-Z]"))
                return "La contraseña debe incluir al menos una mayúscula.";

            if (!Regex.IsMatch(password, "[a-z]"))
                return "La contraseña debe incluir al menos una minúscula.";

            if (!Regex.IsMatch(password, "[0-9]"))
                return "La contraseña debe incluir al menos un número.";

            if (!Regex.IsMatch(password, @"[^A-Za-z0-9\s]"))
                return "La contraseña debe incluir al menos un símbolo.";

            return null;
        }
    }
}
