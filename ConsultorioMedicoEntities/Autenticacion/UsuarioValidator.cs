namespace ConsultorioMedicoEntities.Autenticacion
{
    /// <summary>
    /// Validaciones de negocio de Usuario, compartidas entre MVC y WebApi.
    /// </summary>
    public static class UsuarioValidator
    {
        public static string Validar(Usuario u)
        {
            if (u == null) return "Los datos del usuario son inválidos.";
            if (string.IsNullOrWhiteSpace(u.UserName)) return "El nombre de usuario es requerido.";
            // La contraseña es obligatoria solo en alta (Id == 0); en edición puede quedar
            // vacía para conservar la actual (el SP hace ISNULL(@Password, Password)).
            if (u.Id == 0 && string.IsNullOrWhiteSpace(u.Password)) return "La contraseña es requerida.";
            return null;
        }
    }
}
