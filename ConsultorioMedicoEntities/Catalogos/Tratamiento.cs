namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Entidad Tratamiento. La tabla dbo.Tratamiento no tiene columnas de auditoría.
    /// Se usa por ahora para los dropdowns (módulo Cita).
    /// </summary>
    public class Tratamiento
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? Duracion { get; set; }
        public string Descripcion { get; set; }
        public bool Estatus { get; set; }
    }
}
