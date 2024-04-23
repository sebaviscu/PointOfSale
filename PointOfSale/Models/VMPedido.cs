using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMPedido
    {
        public int IdPedido { get; set; }
        public decimal? ImporteEstimado { get; set; }
        public virtual ICollection<VMPedidoProducto>? Productos { get; set; }
        public EstadoPedido? Estado { get; set; }
        public int Orden => Estado == EstadoPedido.Iniciado ? 1 : Estado == EstadoPedido.Enviado ? 2 : Estado == EstadoPedido.Recibido ? 3 : 4;
        public int? IdProveedorMovimiento { get; set; }
        public VMProveedorMovimiento? ProveedorMovimiento { get; set; }
        public string? Comentario { get; set; }
        public int IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }
        public int IdTienda { get; set; }

        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationDateString { get; set; }
        public string? RegistrationUser { get; set; }

        public int? CantidadProductos { get; set; }
        public string? ImporteEstimadoString { get; set; }
        public DateTime? FechaCerrado { get; set; }
        public string? UsuarioFechaCerrado { get; set; }
        public string? FechaCerradoString { get; set; }
        public decimal? ImporteFinal { get; set; }


        public decimal? ImporteSinIva { get; set; }
        public decimal? Iva { get; set; }
        public decimal? IvaImporte { get; set; }
        public string? NroFactura { get; set; }
        public string? TipoFactura { get; set; }
        public EstadoPago? EstadoPago { get; set; }
        public bool FacturaPendiente { get; set; }


    }
}
