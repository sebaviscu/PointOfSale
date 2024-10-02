using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMLov
    {
        public int? IdLov { get; set; }
        public string? Descripcion { get; set; }

        public bool? Estado { get; set; }

        public LovType? LovType { get; set; }

        public string? LovTypeString => LovType.HasValue ? LovType.ToString() : string.Empty;

        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
