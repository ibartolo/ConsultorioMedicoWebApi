namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Entidad DatosInstitucion. La tabla dbo.DatosInstitucion no tiene columnas de auditoría (POCO plano).
    /// </summary>
    public class DatosInstitucion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? NoTrabajadores { get; set; }
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
        public string LogoRuta { get; set; }
        public string MedicoNombre { get; set; }
        public string MedicoCedula { get; set; }
        public string MedicoEspecialidad { get; set; }
    }
}
