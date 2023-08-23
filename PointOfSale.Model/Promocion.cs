using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class Promocion
    {
        public int IdPromocion { get; set; }
        public string? Nombre { get; set; }
        public string? IdProducto { get; set; }
        public int? Operador { get; set; }
        public int? CantidadProducto { get; set; }
        public string? IdCategory { get; set; }
        public string? Dias { get; set; }
        public decimal? Precio { get; set; }
        public decimal? Porcentaje { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public int IdTienda { get; set; }
    }
}
