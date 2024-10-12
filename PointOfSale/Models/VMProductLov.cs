using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMProductLov
    {
        public int? ProductId { get; set; }

        public int? LovId { get; set; }

        public LovType? LovType { get; set; }
    }
}
