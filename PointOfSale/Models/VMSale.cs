using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMSale
    {
        public int? IdSale { get; set; }
        public string? SaleNumber { get; set; }
        public int? IdTypeDocumentSale { get; set; }
        public string? TypeDocumentSale { get; set; }
        public int? IdUsers { get; set; }
        public string? Users { get; set; }
        public string? CustomerDocument { get; set; }
        public string? ClientName { get; set; }
        public decimal? Total { get; set; }
        public decimal? TotalDecimal { get; set; }
        public string? RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }
        public virtual ICollection<VMDetailSale>? DetailSales { get; set; }
        public int? IdTurno { get; set; }
        public int? ClientId { get; set; }
        public TipoMovimientoCliente? TipoMovimiento { get; set; }
        public int? IdTienda { get; set; }
        public int? CantidadProductos { get; set; }
        public int? IdClienteMovimiento { get; set; }
        public ClienteMovimiento? ClienteMovimiento { get; set; }
        public bool ImprimirTicket { get; set; }
        public List<VMMultiplesFormaPago>? MultiplesFormaDePago { get; set; }
        public int? IdClienteFactura { get; set; }
        public string? CuilFactura { get; set; }
        public string? DescuentoRecargo { get; set; }
        public string? NombreImpresora { get; set; }
        public string? Ticket { get; set; }
        public string? urlQr { get; set; }
        public List<Images> ImagesTicket { get; set; }
        public bool? IsWeb { get; set; }


    }
}
