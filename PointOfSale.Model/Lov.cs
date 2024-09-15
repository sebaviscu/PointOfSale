using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class Lov
    {
        public int IdLov { get; set; }
        public string Descripcion { get; set; }

        public bool Estado { get; set; }

        public LovType LovType { get; set; }

    }

}
