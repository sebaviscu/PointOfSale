using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Web.Models
{
    public class VMDetalleFacturaIva
    {
        public int Id { get; set; }
        public IVA_Afip TipoIva { get; set; }
        public double ImporteNeto { get; set; }
        public double ImporteIVA { get; set; }
        public double ImporteTotal { get; set; }
        public int IdFacturaEmitida { get; set; }
    }
}
