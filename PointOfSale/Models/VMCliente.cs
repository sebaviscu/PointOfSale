using PointOfSale.Model;
using static PointOfSale.Model.Enum;

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
        public decimal? TotalDecimal { get; set; }

        public IEnumerable<VMClienteMovimiento>? ClienteMovimientos { get; set; }

        public string Color => TotalDecimal >= 0 ? "text-success" : "text-danger";
        public int IdTienda { get; set; }
        public CondicionIva? CondicionIva { get; set; }
        public string? Comentario { get; set; }
        public bool IsActive { get; set; }
    }
}
