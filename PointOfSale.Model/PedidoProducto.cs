using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class PedidoProducto
    {
        public int IdPedidoProducto { get; set; }
        public int CantidadProducto { get; set; }
        public int? CantidadProductoRecibida { get; set; }
        public string? Lote { get; set; }
        public DateTime? Vencimiento { get; set; }
        public int IdProducto { get; set; }
        public virtual Product? Product { get; set; }
        public int IdPedido { get; set; }
        public virtual Pedido? Pedido { get; set; }
    }
}
