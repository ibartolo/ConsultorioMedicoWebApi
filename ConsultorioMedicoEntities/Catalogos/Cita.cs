using System;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Entidad Cita. La tabla dbo.Cita NO tiene columnas de auditoría, por lo que es un POCO plano.
    /// </summary>
    public class Cita
    {
        public int Id { get; set; }
        public bool Estatus { get; set; }
        public DateTime? FechaCita { get; set; }
        public int PacienteID { get; set; }
        public string Comentarios { get; set; }
        public int? TratamientoID { get; set; }
        public int? Estado { get; set; }
    }
}
