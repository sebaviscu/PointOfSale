using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMCliente
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string? Cuit { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Email { get; set; }
        public string? CondicionIVA { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public string? Total { get; set; }
        public decimal? TotalDecimal { get; set; }

        public IEnumerable<ClienteMovimiento>? ClienteMovimientos { get; set; }

        public string Color => TotalDecimal >= 0 ? "text-success" : "text-danger";
        public int IdTienda { get; set; }

    }
}
