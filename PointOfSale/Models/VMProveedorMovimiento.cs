using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public partial class VMProveedorMovimiento
    {
        public int IdProveedorMovimiento { get; set; }
        public decimal Importe { get; set; }
        public decimal? ImporteSinIva { get; set; }
        public decimal? Iva { get; set; }
        public decimal? IvaImporte { get; set; }
        public string? NroFactura { get; set; }
        public string? TipoFactura { get; set; }
        public string? Comentario { get; set; }
        public int? idTienda { get; set; }
        public int IdProveedor { get; set; }
        public string? NombreProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationDateString { get; set; }
        public string? RegistrationUser { get; set; }

        public string TipoFacturaString
        {
            get { return (!string.IsNullOrEmpty(TipoFactura) ? ((Model.Enum.TipoFactura)Convert.ToInt32(TipoFactura)).ToString() : string.Empty); }
        }

        public string? ImporteString { get; set; }
        public string? ImporteSinIvaString { get; set; }
        public string? IvaImporteString { get; set; }
        public EstadoPago? EstadoPago { get; set; }
        public int? IdPedido { get; set; }
        public Pedido? Pedido { get; set; }
        public bool FacturaPendiente { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public FormaPagoProveedores? FormaPago { get; set; }

    }
}
