using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMProductSimplificado
    {
        public int IdProduct { get; set; }
        public string? BarCode { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public int? IdCategory { get; set; }
        public string? NameCategory { get; set; }
        public decimal? Quantity { get; set; }
        public string? Price { get; set; }
        public string? PriceString { get; set; }
        public string? PhotoBase64 { get; set; }
        public int? IsActive { get; set; }
        public string? ModificationUser { get; set; }
        public DateTime? ModificationDate { get; set; }
        public decimal? PriceWeb { get; set; }
        public int? PorcentajeProfit { get; set; }
        public decimal? CostPrice { get; set; }
        public TipoVenta TipoVenta { get; set; }
        public string NameProveedor { get; set; }
        public int? IdProveedor { get; set; }
        public string? Comentario { get; set; }
        public decimal? Minimo { get; set; }
        public string? ModificationDateString { get; set; }
        public decimal? Iva { get; set; }
        public string? Precio2 { get; set; }
        public int? PorcentajeProfit2 { get; set; }
        public string? Precio3 { get; set; }
        public int? PorcentajeProfit3 { get; set; }
        public List<VMVencimiento>? Vencimientos { get; set; }
        public List<VMListaPrecio>? ListaPrecios { get; set; }
        public VMProveedorSimplificado? Proveedor { get; set; }
        public virtual VMCategory? IdCategoryNavigation { get; set; }
        public virtual ICollection<VMStockSimplificado>? Stocks { get; set; }
    }
}
