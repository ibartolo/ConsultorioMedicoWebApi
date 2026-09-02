using System;

namespace ConsultorioMedicoEntities
{
    /// <summary>
    /// Clase base de entidades. Refleja las columnas de auditoría reales de la base de datos
    /// (CreatedBY / CreatedDT / UpdatedBY / UpdatedDT) y el campo Estatus.
    /// </summary>
    public class BaseObject
    {
        public int Id { get; set; }
        public string CreatedBY { get; set; }
        public DateTime? CreatedDT { get; set; }
        public string UpdatedBY { get; set; }
        public DateTime? UpdatedDT { get; set; }
        public bool Estatus { get; set; }
    }
}
