namespace PointOfSale.Model
{
    public class ProveedorMovimiento
    {
        public ProveedorMovimiento(int idProveedor, decimal importe, string registrationUser)
        {
            IdProveedor = idProveedor;
            Importe = importe;
            RegistrationDate = DateTime.Now;
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
        public int IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string RegistrationUser { get; set; }
    }
}
