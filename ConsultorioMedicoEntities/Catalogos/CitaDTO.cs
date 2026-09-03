namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// DTO de Cita. Agrega los nombres de paciente y tratamiento (joins del listado).
    /// </summary>
    public class CitaDTO : Cita
    {
        public string PacienteNombre { get; set; }
        public string TratamientoNombre { get; set; }
    }
}
