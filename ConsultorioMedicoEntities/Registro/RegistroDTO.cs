namespace ConsultorioMedicoEntities.Registro
{
    /// <summary>
    /// DTO de captura del registro público de un consultorio (SaaS).
    /// Campos del formulario de registro; no refleja una tabla concreta, sino que
    /// se reparte entre DatosInstitucion, DatosFiscales y Usuario en el alta transaccional.
    /// </summary>
    public class RegistroDTO
    {
        public string NombreComercial { get; set; }
        public string RazonSocial { get; set; }
        public string RFC { get; set; }
        public string Responsable { get; set; }
        public string Calle { get; set; }
        public string NoExterior { get; set; }
        public string NoInterior { get; set; }
        public string Colonia { get; set; }
        public string Municipio { get; set; }
        public string Estado { get; set; }
        public int? CP { get; set; }
        public string Telefono { get; set; }
        public string CorreoContacto { get; set; }
        public string Password { get; set; }
        public string Confirmacion { get; set; }
    }
}
