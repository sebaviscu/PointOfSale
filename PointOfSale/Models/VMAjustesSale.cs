using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMAjustesSale
    {
        public bool? ImprimirDefault { get; set; }
        public long? MinimoIdentificarConsumidor { get; set; }
        public bool? ControlEmpleado { get; set; }
        public bool NeedControl { get; set; }
        public ListaDePrecio ListaPrecios { get; set; }
    }
}
