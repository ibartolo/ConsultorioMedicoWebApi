using System;

namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Entidad ConsultorioPago. La tabla dbo.ConsultorioPago no tiene columnas
    /// de auditoría genéricas (POCO plano). Análoga a EmpresaPago de HelpDesk,
    /// sin RequiereFactura. EstatusPagoId: 1=Pendiente, 2=Confirmado, 3=Rechazado.
    /// </summary>
    public class ConsultorioPago
    {
        public int Id { get; set; }
        public int ConsultorioID { get; set; }
        public string NombreArchivo { get; set; }
        public string ArchivoRuta { get; set; }
        public decimal Monto { get; set; }
        public int EstatusPagoId { get; set; }
        public DateTime FechaCarga { get; set; }
        public string SubidoPor { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public string ConfirmadoPor { get; set; }
        public string Notas { get; set; }
        public bool Estatus { get; set; }
    }
}
