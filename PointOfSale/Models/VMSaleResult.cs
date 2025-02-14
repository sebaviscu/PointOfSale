using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Models
{
    public class VMSaleResult
    {
        public VMSaleResult()
        {
            ImagesTicket = new List<Images>();
            Ticket = string.Empty;
            IdSaleMultiple = string.Empty;
        }

        public int? IdSale { get; set; }
        public string? IdSaleMultiple { get; set; }
        public string? SaleNumber { get; set; }
        public string? NombreImpresora { get; set; }
        public string? Ticket { get; set; }
        public List<Images> ImagesTicket { get; set; }
        public string? Errores { get; set; }
        public string TipoVenta { get; set; }
        public List<FacturaAFIP>? FacturasAFIP { get; set; } = new List<FacturaAFIP>();
        public bool Facturar => FacturasAFIP != null && FacturasAFIP.Any();

    }
}
