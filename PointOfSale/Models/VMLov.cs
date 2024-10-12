using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMLov : EntityBase
    {
        public string? Descripcion { get; set; }

        public bool? Estado { get; set; }

        public LovType? LovType { get; set; }

        public string? LovTypeString => LovType.HasValue ? LovType.ToString() : string.Empty;

    }
}
