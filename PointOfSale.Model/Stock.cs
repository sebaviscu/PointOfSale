using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class Stock
    {
        public Stock()
        {
                
        }
        public Stock(decimal stockActual, int stockMinimo, int idProducto, int idTienda)
        {
            StockActual = stockActual;
            StockMinimo = stockMinimo;
            IdProducto = idProducto;
            IdTienda = idTienda;
        }

        public int IdStock { get; set; }
        public decimal StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int IdProducto { get; set; }
        public Product? Producto { get; set; }
        public int IdTienda { get; set; }
        public Tienda? Tienda { get; set; }
    }
}
