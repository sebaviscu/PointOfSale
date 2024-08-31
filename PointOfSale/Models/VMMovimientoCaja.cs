using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMMovimientoCaja
    {
        public int IdMovimientoCaja { get; set; }
        public string Comentario { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }
        public decimal Importe { get; set; }
        public string IdRazonMovimientoCaja { get; set; }
        public int? IdTienda { get; set; }
        public int IdTurno { get; set; }
        public virtual VMRazonMovimientoCaja? RazonMovimientoCaja { get; set; }
    }
}
