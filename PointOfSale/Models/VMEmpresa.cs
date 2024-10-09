using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMEmpresa : EntityBaseDto
    {
        public string? RazonSocial { get; set; }
        public string? NombreContacto { get; set; }
        public string? NumeroContacto { get; set; }
        public Licencias? Licencia { get; set; }
        public DateTime? ProximoPago { get; set; }
        public FrecuenciasPago? FrecuenciaPago { get; set; }
        public string? Comentario { get; set; }
        public virtual List<VMPagoEmpresa>? Pagos { get; set; }
    }
}
