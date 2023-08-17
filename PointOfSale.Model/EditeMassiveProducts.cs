using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class EditeMassiveProducts
    {
        public string? Precio { get; set; }
        public List<int> idProductos { get; set; }
        public string? PriceWeb { get; set; }
        public string? Profit { get; set; }
        public string? Costo { get; set; }
        public bool? IsActive { get; set; }
    }
}
