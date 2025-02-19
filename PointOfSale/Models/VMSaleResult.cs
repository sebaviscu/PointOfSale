using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Models
{
    public class VMSaleResult
    {

        public int? IdSale { get; set; }
        public string? IdSaleMultiple { get; set; } = string.Empty;
        public string? SaleNumber { get; set; }
        public string? NombreImpresora { get; set; }
        public string? Ticket { get; set; } = string.Empty;
        public List<Images> ImagesTicket { get; set; } = new List<Images>();
        public string? Errores { get; set; }
        public string TipoVenta { get; set; }
        public FacturaAFIP? FacturaAFIP { get; set; }
        public bool Facturar => FacturaAFIP != null;

    }
}
