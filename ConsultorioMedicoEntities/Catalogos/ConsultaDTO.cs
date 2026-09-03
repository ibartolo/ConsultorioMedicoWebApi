namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// DTO de Consulta. Agrega nombres de paciente y tipo de consulta (joins del listado).
    /// </summary>
    public class ConsultaDTO : Consulta
    {
        public string PacienteNombre { get; set; }
        public string TipoConsultaNombre { get; set; }
    }
}
