namespace PointOfSale.Models
{
    public class VMDetailSale
    {
        public int? IdProduct { get; set; }
        public string? BrandProduct { get; set; }
        public string? DescriptionProduct { get; set; }
        public string? CategoryProducty { get; set; }
        public decimal? Quantity { get; set; }
        public string? Price { get; set; }
        public string? Total { get; set; }
        public string? Promocion { get; set; }
        public int? row { get; set; }
    }
}
