using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMCliente
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string? Cuil { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public string? Total { get; set; }
        public IEnumerable<ClienteMovimiento>? ClienteMovimientos { get; set; }

        public string Color { get; set; }
        public int IdTienda { get; set; }

    }
}
