using System;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Entidad Paciente. Refleja la tabla dbo.Paciente.
    /// Las columnas de auditoría (CreatedBY/CreatedDT/UpdatedBY/UpdatedDT) existen en la tabla
    /// pero la app WinForms NO las escribe (quedan NULL); se conservan por fidelidad al esquema.
    /// </summary>
    public class Paciente : BaseObject
    {
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Email { get; set; }
        public DateTime? FechaAlta { get; set; }
        public string TelefonoCel { get; set; }
        public string Comentarios { get; set; }
        public int? Edad { get; set; }
        public string Sexo { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }
}
