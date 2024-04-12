using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public decimal? ImporteEstimado { get; set; }
        public EstadoPedido Estado { get; set; }
        public int? IdProveedorMovimiento { get; set; }
        public ProveedorMovimiento? ProveedorMovimiento { get; set; }
        public string? Comentario { get; set; }
        public int IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }
        public int IdTienda { get; set; }
        public Tienda? Tienda { get; set; }
        public virtual ICollection<PedidoProducto>? Productos { get; set; }

        public DateTime RegistrationDate { get; set; }
        public string RegistrationUser { get; set; }

        public DateTime? FechaCerrado { get; set; }
        public string? UsuarioFechaCerrado { get; set; }
        public decimal? ImporteFinal { get; set; }

    }
}
