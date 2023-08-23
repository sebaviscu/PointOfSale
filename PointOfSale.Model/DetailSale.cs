using System;
using System.Collections.Generic;

namespace PointOfSale.Model
{
    public partial class DetailSale
    {
        public int IdDetailSale { get; set; }
        public int? IdSale { get; set; }
        public int? IdProduct { get; set; }
        public string? BrandProduct { get; set; }
        public string? DescriptionProduct { get; set; }
        public string? CategoryProducty { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Total { get; set; }

        public virtual Sale? IdSaleNavigation { get; set; }
        public virtual Product? Producto { get; set; }
        public string? Promocion { get; set; }
    }
}
