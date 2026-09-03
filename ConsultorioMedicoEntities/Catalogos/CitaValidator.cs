namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Validaciones de negocio de Cita, compartidas entre MVC y WebApi.
    /// </summary>
    public static class CitaValidator
    {
        public static string Validar(Cita c)
        {
            if (c == null) return "Los datos de la cita son inválidos.";
            if (c.PacienteID <= 0) return "Seleccione el paciente.";
            if (!c.FechaCita.HasValue) return "La fecha de la cita es requerida.";
            return null;
        }
    }
}
