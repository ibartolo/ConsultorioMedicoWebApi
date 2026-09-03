using System;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Entidad Consulta. La tabla dbo.Consulta no tiene columnas de auditoría (POCO plano).
    /// </summary>
    public class Consulta
    {
        public int Id { get; set; }
        public bool Estatus { get; set; }
        public DateTime? FechaConsulta { get; set; }
        public int PacienteID { get; set; }
        public int? TratamientoID { get; set; }
        public int? CitaID { get; set; }
        public string Descripcion { get; set; }
        public int? TipoConsultaID { get; set; }
        public string DescripcionReceta { get; set; }
        public string Comentarios { get; set; }
        public DateTime? FechaProximaConsulta { get; set; }
    }
}
