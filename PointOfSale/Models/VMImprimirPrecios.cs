using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMImprimirPrecios
    {
        public List<int> IdProductos { get; set; }
        public ListaDePrecio ListaPrecio { get; set; }
        public bool FechaModificacion { get; set; }
        public bool CodigoBarras { get; set; }
    }
}
