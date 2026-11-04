namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Validaciones de negocio de TipoConsulta, compartidas entre MVC y WebApi.
    /// </summary>
    public static class TipoConsultaValidator
    {
        public static string Validar(TipoConsulta t)
        {
            if (t == null) return "Los datos del tipo de consulta son inválidos.";
            if (string.IsNullOrWhiteSpace(t.Nombre)) return "El nombre es requerido.";
            return null;
        }
    }
}
