namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// DTO de Paciente. Agrega NombreCompleto, que el SP GetAllPacienteByEstatus calcula al vuelo.
    /// </summary>
    public class PacienteDTO : Paciente
    {
        public string NombreCompleto { get; set; }
    }
}
