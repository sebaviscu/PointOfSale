using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMVencimiento
    {
        public int IdVencimiento { get; set; }
        public string? Lote { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaElaboracion { get; set; }
        public bool Notificar { get; set; }

        public int IdProducto { get; set; }

        public int IdTienda { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }
    }
}
