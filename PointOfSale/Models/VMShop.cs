using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMShop
    {
        public List<VMProduct> Products { get; set; }

        public VMTienda Tienda { get; set; }

        public bool IsLogin { get; set; }
    }
}
