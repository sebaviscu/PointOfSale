using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMPagoEmpresa : EntityBaseDto
    {
        public int? IdEmpresa { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal? Importe { get; set; }
        public string? Comentario { get; set; }
        public EstadoPago EstadoPago { get; set; }
        public string TipoFacturaString => TipoFactura.ToString();

        public decimal? ImporteSinIva { get; set; }
        public decimal? Iva { get; set; }
        public decimal? IvaImporte { get; set; }
        public string? NroFactura { get; set; }
        public TipoFactura? TipoFactura { get; set; }
        public bool? FacturaPendiente { get; set; }
    }
}
