using System;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// DTO del historial de tratamientos de un paciente (datos del SP sp_nh_GetTratamientosPorPaciente).
    /// </summary>
    public class TratamientoHistoricoDTO
    {
        public int Id { get; set; }
        public DateTime? Fecha { get; set; }
        public string TratamientoNombre { get; set; }
        public string Origen { get; set; }
        public string Comentarios { get; set; }
    }
}
