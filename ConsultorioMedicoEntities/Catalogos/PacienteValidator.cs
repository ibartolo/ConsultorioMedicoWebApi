using System.Text.RegularExpressions;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Validaciones de negocio de Paciente, compartidas entre MVC y WebApi.
    /// Se repiten en ambas capas para proteger contra peticiones manipuladas
    /// y para la futura app móvil.
    /// </summary>
    public static class PacienteValidator
    {
        public static string Validar(Paciente p)
        {
            if (p == null) return "Los datos del paciente son inválidos.";
            if (string.IsNullOrWhiteSpace(p.Nombre)) return "El nombre es requerido.";
            if (string.IsNullOrWhiteSpace(p.ApellidoPaterno)) return "El apellido paterno es requerido.";
            if (!Regex.IsMatch(p.Nombre, @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]+$")) return "El nombre solo puede contener letras y espacios.";
            if (!Regex.IsMatch(p.ApellidoPaterno, @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]+$")) return "El apellido paterno solo puede contener letras y espacios.";
            if (!string.IsNullOrWhiteSpace(p.ApellidoMaterno) && !Regex.IsMatch(p.ApellidoMaterno, @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]+$")) return "El apellido materno solo puede contener letras y espacios.";
            if (string.IsNullOrWhiteSpace(p.Sexo)) return "Seleccione el sexo.";
            if (string.IsNullOrWhiteSpace(p.TelefonoCel)) return "El teléfono es requerido.";
            if (!Regex.IsMatch(p.TelefonoCel, @"^\d{10}$")) return "El teléfono debe tener exactamente 10 dígitos.";
            if (!string.IsNullOrWhiteSpace(p.Email) && !Regex.IsMatch(p.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return "Ingrese un formato de correo válido.";
            return null;
        }
    }
}
