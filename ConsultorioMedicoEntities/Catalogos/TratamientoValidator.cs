namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Validaciones de negocio de Tratamiento, compartidas entre MVC y WebApi.
    /// </summary>
    public static class TratamientoValidator
    {
        public static string Validar(Tratamiento t)
        {
            if (t == null) return "Los datos del tratamiento son inválidos.";
            if (string.IsNullOrWhiteSpace(t.Nombre)) return "El nombre es requerido.";
            return null;
        }
    }
}
