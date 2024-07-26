using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMListaPrecio
    {
        public int IdListaPrecios { get; set; }
        public ListaDePrecio Lista { get; set; }
        public int IdProducto { get; set; }
        public decimal Precio { get; set; }
        public int PorcentajeProfit { get; set; }
        public DateTime? RegistrationDate { get; set; }
    }
}
