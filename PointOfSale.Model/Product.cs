using System;
using System.Collections.Generic;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public partial class Product
    {
        public int IdProduct { get; set; }
        public string? BarCode { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public int? IdCategory { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
        public byte[]? Photo { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? RegistrationDate { get; set; }
		public DateTime? ModificationDate { get; set; }
		public string? ModificationUser { get; set; }
        public decimal? PriceWeb { get; set; }
        public int? PorcentajeProfit { get; set; }
        public decimal? CostPrice { get; set; }
        public string? Comentario { get; set; }
        public TipoVenta TipoVenta { get; set; }
        public decimal? Minimo { get; set; }
        public virtual Category? IdCategoryNavigation { get; set; }
        public int? IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }
        public IEnumerable<DetailSale>? DetalleVentas { get; set; }
        public List<ListaPrecio>? ListaPrecios { get; set; }
    }
}
