using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public partial class Product
    {
        public int IdProduct { get; set; }
        public string? SKU { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public byte[]? Photo { get; set; }
        public bool IsActive { get; set; }
        public DateTime? RegistrationDate { get; set; }
		public DateTime? ModificationDate { get; set; }
		public string? ModificationUser { get; set; }
        public decimal? PriceWeb { get; set; }
        public int? PorcentajeProfit { get; set; }
        public decimal? CostPrice { get; set; }
        public string? Comentario { get; set; }
        public TipoVenta TipoVenta { get; set; }
        public decimal? Iva {  get; set; }
        public decimal? PrecioFormatoWeb { get; set; }
        public int? FormatoWeb { get; set; }
        public bool Destacado { get; set; }
        public bool ProductoWeb { get; set; }
        public bool? ModificarPrecio { get; set; }
        public bool? PrecioAlMomento { get; set; }
        public bool? ExcluirPromociones { get; set; }

        public int? IdCategory { get; set; }
        public virtual Category? IdCategoryNavigation { get; set; }

        public int? IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }

        public virtual List<DetailSale>? DetalleVentas { get; set; } = new List<DetailSale>();
        public virtual List<ListaPrecio>? ListaPrecios { get; set; } = new List<ListaPrecio>();
        public virtual List<Vencimiento>? Vencimientos { get; set; } = new List<Vencimiento>();
        public virtual List<PedidoProducto>? PedidoProductos { get; set; } = new List<PedidoProducto>();
        public virtual List<Stock>? Stocks { get; set; } = new List<Stock>();
        public virtual List<CodigoBarras>? CodigoBarras { get; set; } = new List<CodigoBarras>();
        public virtual List<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
        public virtual List<ProductLov>? ProductLovs { get; set; } = new List<ProductLov>();

    }
}
