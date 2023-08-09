using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMPromocion
    {
        public int IdPromocion { get; set; }
        public string? Nombre { get; set; }
        public string? IdProducto { get; set; }
        public int? Operador { get; set; }
        public int? CantidadProducto { get; set; }
        public int[]? IdCategory { get; set; }
        public int[]? Dias { get; set; }
        public decimal? Precio { get; set; }
        public decimal? Porcentaje { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public string? Promocion { get; set; }
        public int? Row { get; set; }
        public decimal Total { get; set; }
    }
}
