using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMTurno
    {
        public int IdTurno { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Descripcion { get; set; }
        public int? IdTienda { get; set; }
        public Tienda? Tienda { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }
        public string? ModificationUser { get; set; }
        public List<VMVentasPorTipoDeVenta>? VentasPorTipoVenta { get; set; }
        public string? Fecha { get; set; }
        public string? HoraInicio { get; set; }
        public string? HoraFin { get; set; }

        public string? Total { get; set; }

        public Sale? Sale { get; set; }

    }
}
