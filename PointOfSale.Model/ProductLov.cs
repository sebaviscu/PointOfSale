using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class ProductLov
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int LovId { get; set; }
        public Lov? Lov { get; set; }

        public LovType LovType { get; set; }
    }
}
