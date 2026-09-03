using System;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// DTO de la receta de una consulta (datos del SP GetRecetaByConsulta).
    /// </summary>
    public class RecetaDTO
    {
        public string Paciente { get; set; }
        public DateTime? FechaConsulta { get; set; }
        public string Consulta { get; set; }
        public string Medicamento { get; set; }
        public string TipoConsulta { get; set; }
        public DateTime? ProximaConsulta { get; set; }
    }
}
