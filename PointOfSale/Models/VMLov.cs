using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMLov
    {
        public int LovId { get; set; }
        public string Descripcion { get; set; }

        public bool Estado { get; set; }

        public LovType LovType { get; set; }

        public string LovTypeString => LovType.ToString();
    }
}
