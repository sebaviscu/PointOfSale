using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMPedidoProducto
    {
        public int IdPedidoProducto { get; set; }
        public int? IdProducto { get; set; }
        public int? CantidadProducto { get; set; }
        public int? IdPedido { get; set; }
        public string? Lote { get; set; }
        public string? Vencimiento { get; set; }
        public int? CantidadProductoRecibida { get; set; }
        public string? Description { get; set; }
        public string? CostPrice { get; set; }
        public virtual ICollection<Stock>? Stocks { get; set; }

    }
}
