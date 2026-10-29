namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Entidad DatosFiscales. La tabla dbo.DatosFiscales no tiene columnas de auditoría (POCO plano).
    /// DatosInstitucionNombre es un campo de solo lectura que se llena con el join al listar.
    /// </summary>
    public class DatosFiscales
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; }
        public string RFC { get; set; }
        public string Telefono1 { get; set; }
        public string Telefono2 { get; set; }
        public string Email { get; set; }
        public string Calle { get; set; }
        public string NoInterior { get; set; }
        public string NoExterior { get; set; }
        public string Colonia { get; set; }
        public string Deleacion { get; set; }
        public string Municipio { get; set; }
        public string Estado { get; set; }
        public int? CP { get; set; }
        public string Pais { get; set; }
        public int? DatosInstitucionID { get; set; }
        public string DatosInstitucionNombre { get; set; }
    }
}
