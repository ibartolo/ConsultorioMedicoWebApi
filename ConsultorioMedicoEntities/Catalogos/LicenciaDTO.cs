using System;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// DTO de licencia. Refleja la salida de sp_nh_GetLicencia
    /// (Id AS ConsultorioID + columnas de licencia de DatosInstitucion).
    /// </summary>
    public class LicenciaDTO
    {
        public int ConsultorioID { get; set; }
        public string Nombre { get; set; }
        public bool Estatus { get; set; }
        public DateTime? FechaVigenciaInicio { get; set; }
        public DateTime? FechaVigenciaFin { get; set; }
        public bool EsPeriodoPrueba { get; set; }
        public string Plan { get; set; }
    }
}
