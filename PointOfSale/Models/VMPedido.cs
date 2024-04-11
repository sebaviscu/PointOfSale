using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMPedido
    {
        public int IdPedido { get; set; }
        public decimal? ImporteEstimado { get; set; }
        public virtual ICollection<PedidoProducto>? Productos { get; set; }
        public EstadoPedido? Estado { get; set; }
        public int? IdProveedorMovimiento { get; set; }
        public ProveedorMovimiento? ProveedorMovimiento { get; set; }
        public string? Comentario { get; set; }
        public int IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }
        public int IdTienda { get; set; }

        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationDateString { get; set; }
        public string? RegistrationUser { get; set; }

        public int? CantidadProductos { get; set; }
        public string? ImporteEstimadoString { get; set; }

    }
}
