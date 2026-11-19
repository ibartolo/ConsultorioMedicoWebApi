using System.Text.RegularExpressions;

namespace ConsultorioMedicoEntities.Autenticacion
{
    /// <summary>
    /// Validaciones de negocio de Usuario, compartidas entre MVC y WebApi.
    /// </summary>
    public static class UsuarioValidator
    {
        private const string PatronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        public static string Validar(Usuario u)
        {
            if (u == null) return "Los datos del usuario son inválidos.";
            if (string.IsNullOrWhiteSpace(u.UserName)) return "El nombre de usuario es requerido.";
            // La contraseña es obligatoria solo en alta (Id == 0); en edición puede quedar
            // vacía para conservar la actual (el SP hace ISNULL(@Password, Password)).
            if (u.Id == 0 && string.IsNullOrWhiteSpace(u.Password)) return "La contraseña es requerida.";

            // El correo es opcional; si viene, debe tener formato válido y no exceder 250.
            if (!string.IsNullOrWhiteSpace(u.Email))
            {
                if (u.Email.Length > 250) return "El correo no puede exceder 250 caracteres.";
                if (!Regex.IsMatch(u.Email, PatronEmail)) return "Ingrese un formato de correo válido.";
            }
            return null;
        }
    }
}
