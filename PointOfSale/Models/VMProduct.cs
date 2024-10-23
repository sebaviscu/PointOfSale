using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMProduct
    {
        public int IdProduct { get; set; }
        public string? BarCode { get; set; }
        public string? Description { get; set; }
        public int? IdCategory { get; set; }
        public string? NameCategory { get; set; }
        public decimal? Quantity { get; set; }
        public string? Price { get; set; }
        public string? PriceString { get; set; }
        public byte[]? Photo { get; set; }
        public string? PhotoBase64 { get; set; }
        public int? IsActive { get; set; }
		public string? ModificationUser { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? PriceWeb { get; set; }
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
        public string? PrecioFormatoWeb { get; set; }
        public int? FormatoWeb { get; set; }
        public bool Destacado { get; set; }
        public bool ProductoWeb { get; set; }
        public bool? ModificarPrecio { get; set; }
        public bool? PrecioAlMomento { get; set; }
        public bool? ExcluirPromociones { get; set; }

        public virtual ICollection<Stock>? Stocks { get; set; }
        public virtual ICollection<VMTag>? Tags { get; set; }
        public List<VMVencimiento>? Vencimientos { get; set; }
        public List<VMListaPrecio>? ListaPrecios { get; set; }
        public virtual ICollection<VMCodigoBarras>? CodigoBarras { get; set; }
        public VMProveedor? Proveedor { get; set; }
        public virtual VMCategory? IdCategoryNavigation { get; set; }
        public virtual ICollection<VMLov>? Comodin1 { get; set; }
        public virtual ICollection<VMLov>? Comodin2 { get; set; }
        public virtual ICollection<VMLov>? Comodin3 { get; set; }

    }
}
