using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMVentaWeb
    {
        public int IdVentaWeb { get; set; }
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Comentario { get; set; }
        public int? IdFormaDePago { get; set; }
        public int? IdUsers { get; set; }
        public decimal? Total { get; set; }
        public string? TotalString { get; set; }
        public EstadoVentaWeb? Estado { get; set; }
        public int? IdTienda { get; set; }
        public virtual ICollection<DetailSale>? DetailSales { get; set; }
        public string? Fecha { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? FormaDePago { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }

        public bool? IsEdit { get; set; }
        public string? EditText { get; set; }

    }
}
