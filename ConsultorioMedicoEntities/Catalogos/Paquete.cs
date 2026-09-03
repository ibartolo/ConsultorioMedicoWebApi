using System;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Entidad Paquete. La tabla dbo.Paquete no tiene columnas de auditoría (POCO plano).
    /// </summary>
    public class Paquete
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public bool Estatus { get; set; }
        public int? Costo { get; set; }
    }
}
