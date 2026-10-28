using System;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// DTO de lectura de las citas-tratamientos de un paquete del paciente (SP sp_nh_GetCitasPorPacientePaquete).
    /// </summary>
    public class CitaPaqueteLecturaDTO
    {
        public int Id { get; set; }
        public DateTime? FechaCita { get; set; }
        public string TratamientoNombre { get; set; }
        public string Comentarios { get; set; }
        public int CitaPaqueteID { get; set; }
        public int PacientePaqueteID { get; set; }
    }
}
