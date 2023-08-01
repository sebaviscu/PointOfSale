using PointOfSale.Model;

namespace PointOfSale.Models
{
    public partial class VMProveedor
    {
        public int IdProveedor { get; set; }
        public string Nombre { get; set; }
        public string? Cuil { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }

    }
}
