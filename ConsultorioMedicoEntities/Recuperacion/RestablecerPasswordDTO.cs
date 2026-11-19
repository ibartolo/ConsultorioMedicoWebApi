namespace ConsultorioMedicoEntities.Recuperacion
{
    /// <summary>
    /// DTO de captura para restablecer la contraseña usando un token de recuperación.
    /// </summary>
    public class RestablecerPasswordDTO
    {
        public string Token { get; set; }
        public string Password { get; set; }
        public string Confirmacion { get; set; }
    }
}
