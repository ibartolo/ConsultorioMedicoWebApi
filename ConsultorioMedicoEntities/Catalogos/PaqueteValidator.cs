namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Validaciones de negocio de Paquete, compartidas entre MVC y WebApi.
    /// </summary>
    public static class PaqueteValidator
    {
        public static string Validar(Paquete p)
        {
            if (p == null) return "Los datos del paquete son inválidos.";
            if (string.IsNullOrWhiteSpace(p.Nombre)) return "El nombre es requerido.";
            return null;
        }
    }
}
