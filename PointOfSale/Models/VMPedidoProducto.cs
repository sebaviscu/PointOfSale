namespace PointOfSale.Models
{
    public class VMPedidoProducto
    {
        public int IdPedidoProducto { get; set; }
        public int? IdProducto { get; set; }
        public int? CantidadProducto { get; set; }
        public int? IdPedido { get; set; }

    }
}
