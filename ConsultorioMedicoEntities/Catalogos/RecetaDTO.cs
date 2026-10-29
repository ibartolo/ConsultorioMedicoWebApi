using System;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// DTO de la receta de una consulta (datos del SP GetRecetaByConsulta).
    /// </summary>
    public class RecetaDTO
    {
        public string Paciente { get; set; }
        public int? PacienteEdad { get; set; }
        public string PacienteSexo { get; set; }
        public DateTime? FechaConsulta { get; set; }
        public string Consulta { get; set; }
        public string Medicamento { get; set; }
        public string TipoConsulta { get; set; }
        public DateTime? ProximaConsulta { get; set; }
        public string InstitucionNombre { get; set; }
        public string InstitucionCalle { get; set; }
        public string InstitucionNoInterior { get; set; }
        public string InstitucionNoExterior { get; set; }
        public string InstitucionColonia { get; set; }
        public string InstitucionDeleacion { get; set; }
        public string InstitucionMunicipio { get; set; }
        public string InstitucionEstado { get; set; }
        public int? InstitucionCP { get; set; }
        public string InstitucionPais { get; set; }
        public string InstitucionTelefono1 { get; set; }
        public string InstitucionTelefono2 { get; set; }
        public string InstitucionEmail { get; set; }
        public string LogoRuta { get; set; }
        public string MedicoNombre { get; set; }
        public string MedicoCedula { get; set; }
        public string MedicoEspecialidad { get; set; }
    }
}
