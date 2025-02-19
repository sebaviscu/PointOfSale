using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Model.Output
{
    public class RegisterSaleOutput
    {
        public RegisterSaleOutput()
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
        public string? ErrorFacturacion { get; set; }

        public Ajustes Ajustes { get; set; }
        public List<Sale> SaleList { get; set; } = new List<Sale>();
        public FacturaAFIP FacturaAFIP { get; set; } = new FacturaAFIP();
        public int PuntoVenta { get; set; }
    }
}
