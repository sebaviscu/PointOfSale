using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VmProductsSelect2
    {
        public int IdProduct { get; set; }
        public string? Description { get; set; }
        public int? IdCategory { get; set; }
        public string? Price { get; set; }
         public string? PhotoBase64 { get; set; }
        public TipoVenta TipoVenta { get; set; }
        public string? CategoryProducty { get; set; }
    }
}
