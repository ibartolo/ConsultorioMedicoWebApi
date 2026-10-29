namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Indicadores resumidos del dashboard para el mes en curso.
    /// </summary>
    public class DashboardDTO
    {
        public int PacientesNuevosMes { get; set; }
        public int ConsultasMes { get; set; }
        public int TratamientosMes { get; set; }
        public string Mes { get; set; }
    }
}
