using System;

namespace ConsultorioMedicoEntities.Autenticacion
{
    /// <summary>
    /// Entidad Usuario. La tabla dbo.Usuario NO tiene columnas de auditoría,
    /// por lo que no hereda de BaseObject (es un POCO plano).
    /// </summary>
    public class Usuario
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool Estatus { get; set; }
        public int? CompaniaID { get; set; }

        // Licencia del consultorio (LEFT JOIN en sp_nh_AutenticacionUsuario).
        public DateTime? FechaVigenciaFin { get; set; }
        public bool EsPeriodoPrueba { get; set; }
        public string Plan { get; set; }
        public bool? LicenciaEstatus { get; set; }
    }
}
