using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMDetailSale
    {
        public int? IdProduct { get; set; }
        public string? BrandProduct { get; set; }
        public string? DescriptionProduct { get; set; }
        public string? CategoryProducty { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Total { get; set; }
        public string? Promocion { get; set; }
        public int? row { get; set; }
        public string? diferenciapromocion { get; set; }
        public TipoVenta TipoVenta { get; set; }

    }
}
