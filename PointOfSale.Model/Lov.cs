using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class Lov
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }

        public bool Estado { get; set; }

        public LovType LovType { get; set; }

        public DateTime RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }

}
