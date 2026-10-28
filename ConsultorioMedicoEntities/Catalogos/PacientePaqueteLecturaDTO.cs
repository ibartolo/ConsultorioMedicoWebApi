namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// DTO de lectura de los paquetes adquiridos por un paciente (SP sp_nh_GetPaquetesPorPaciente).
    /// </summary>
    public class PacientePaqueteLecturaDTO
    {
        public int Id { get; set; }
        public int PacienteID { get; set; }
        public int PaqueteID { get; set; }
        public string PaqueteNombre { get; set; }
    }
}
