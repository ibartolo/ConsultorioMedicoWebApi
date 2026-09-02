namespace ConsultorioMedicoEntities.Autenticacion
{
    /// <summary>
    /// DTO de Usuario. Hereda de Usuario y agrega campos de display (joins).
    /// En esta fase CompaniaNombre queda nulo; se poblará cuando el SP haga join con DatosInstitucion.
    /// </summary>
    public class UsuarioDTO : Usuario
    {
        public string CompaniaNombre { get; set; }
    }
}
