using PointOfSale.Business.Utilities;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class ProveedorMovimiento
    {
        public ProveedorMovimiento(int idProveedor, decimal importe, string registrationUser)
        {
            IdProveedor = idProveedor;
            Importe = importe;
            RegistrationDate = TimeHelper.GetArgentinaTime();
            RegistrationUser = registrationUser;
        }

        public int IdProveedorMovimiento { get; set; }
        public decimal Importe { get; set; }
        public decimal? ImporteSinIva { get; set; }
        public decimal? Iva { get; set; }
        public decimal? IvaImporte { get; set; }
        public string? NroFactura { get; set; }
        public string? TipoFactura { get; set; }
        public string? Comentario { get; set; }
        public int idTienda { get; set; }
        public Tienda Tienda { get; set; }
        public int IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string RegistrationUser { get; set; }
        public EstadoPago? EstadoPago { get; set; }
        public int? IdPedido { get; set; }
        public Pedido? Pedido { get; set; }
        public bool FacturaPendiente { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public FormaPagoProveedores? FormaPago { get; set; }
    }
}
