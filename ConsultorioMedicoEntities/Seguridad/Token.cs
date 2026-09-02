using System;

namespace ConsultorioMedicoEntities.Seguridad
{
    /// <summary>
    /// Espejo del JSON OAuth (minúsculas) + expiración local computada por el cliente.
    /// El token NO es JWT: es un blob opaco OAuth 2.0 cifrado con el machineKey.
    /// </summary>
    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
