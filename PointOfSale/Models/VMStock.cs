using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMStock
    {
        public int IdStock { get; set; }
        public decimal StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int IdProducto { get; set; }
        //public VMProduct? Producto { get; set; }
        public int IdTienda { get; set; }
        //public VMTienda? Tienda { get; set; }
    }
}
