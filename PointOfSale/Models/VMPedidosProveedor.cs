using PointOfSale.Model;

namespace PointOfSale.Models
{
    public partial class VMPedidosProveedor
    {
        public int IdProveedor { get; set; }
        public string? Nombre { get; set; }
       
        public ICollection<VMPedidoProducto>? Products { get; set; }
    }
}
