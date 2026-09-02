namespace ConsultorioMedicoEntities.Seguridad
{
    /// <summary>
    /// DTO que viaja DENTRO de la cookie FormsAuth del front (UserData serializado a JSON).
    /// </summary>
    public class TokenCookie
    {
        public Token Token { get; set; }
        public long UserID { get; set; }
        public long EmpresaID { get; set; }
        public string UserName { get; set; }
        public string ProfileImage { get; set; }
        public string UserAvatar { get; set; }
    }
}
