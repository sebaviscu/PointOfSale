using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMGastos
    {
        public int IdGastos { get; set; }
        public int IdTipoGasto { get; set; }
        public decimal Importe { get; set; }
        public int? IdUsuario { get; set; }
        public string? Comentario { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }

        public string? TipoGastoString { get; set; }
        public string? GastoParticular { get; set; }
        public string? ImporteString { get; set; }
        public string? FechaString { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
