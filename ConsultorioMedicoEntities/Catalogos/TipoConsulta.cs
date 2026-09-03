namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Entidad TipoConsulta. La tabla dbo.TipoConsulta no tiene columnas de auditoría.
    /// Se usa para los dropdowns (módulo Consulta).
    /// </summary>
    public class TipoConsulta
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? Duracion { get; set; }
        public string Descripcion { get; set; }
        public bool Estatus { get; set; }
    }
}
