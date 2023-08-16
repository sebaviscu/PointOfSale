using PointOfSale.Model;

namespace PointOfSale.Models
{
    public partial class VMProveedorMovimiento
    {
        public int IdProveedorMovimiento { get; set; }
        public decimal Importe { get; set; }
        public decimal? ImporteSinIva { get; set; }
        public int? Iva { get; set; }
        public decimal? IvaImporte { get; set; }
        public string? NroFactura { get; set; }
        public string? TipoFactura { get; set; }
        public string? Comentario { get; set; }
        public int? idTienda { get; set; }
        public int IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }
    }
}
