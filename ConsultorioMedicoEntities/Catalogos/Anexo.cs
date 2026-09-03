using System;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Entidad Anexo. La tabla dbo.Anexo no tiene columnas de auditoría (POCO plano).
    /// </summary>
    public class Anexo
    {
        public int Id { get; set; }
        public bool Estatus { get; set; }
        public DateTime? Fecha { get; set; }
        public string Comentarios { get; set; }
        public int PacienteID { get; set; }
        public string Nombre { get; set; }
        public string Ruta { get; set; }
        public int? ConsultaID { get; set; }
    }
}
