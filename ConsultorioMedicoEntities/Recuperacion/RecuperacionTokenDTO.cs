using System;

namespace ConsultorioMedicoEntities.Recuperacion
{
    /// <summary>
    /// DTO de lectura de un token de recuperación de contraseña junto con los datos del usuario
    /// asociado. Refleja el resultado de sp_nh_GetTokenRecuperacion (join RecuperacionPasswordToken + Usuario).
    /// </summary>
    public class RecuperacionTokenDTO
    {
        public int TokenId { get; set; }
        public int UsuarioId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Usado { get; set; }
        public DateTime? FechaUso { get; set; }
        /// <summary>El SP lo calcula con la hora de la BD: 1 = no usado y no expirado.</summary>
        public bool Vigente { get; set; }
    }
}
