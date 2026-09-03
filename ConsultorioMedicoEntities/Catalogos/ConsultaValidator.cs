namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Validaciones de negocio de Consulta, compartidas entre MVC y WebApi.
    /// </summary>
    public static class ConsultaValidator
    {
        public static string Validar(Consulta c)
        {
            if (c == null) return "Los datos de la consulta son inválidos.";
            if (c.PacienteID <= 0) return "Seleccione el paciente.";
            if (!c.FechaConsulta.HasValue) return "La fecha de la consulta es requerida.";
            return null;
        }
    }
}
