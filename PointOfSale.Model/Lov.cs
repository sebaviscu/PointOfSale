using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class Lov : EntityBase
    {
        public string Descripcion { get; set; }

        public bool Estado { get; set; }

        public LovType LovType { get; set; }

        public virtual List<ProductLov>? ProductLovs { get; set; } = new List<ProductLov>();
    }
}
