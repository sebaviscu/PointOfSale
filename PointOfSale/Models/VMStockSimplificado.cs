using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMStockSimplificado
    {
        public int IdStock { get; set; }
        public decimal StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int IdProducto { get; set; }
        public int IdTienda { get; set; }
    }
}
