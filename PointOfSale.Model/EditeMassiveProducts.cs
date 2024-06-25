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
        public string? Comentario { get; set; }
        public string? PorPorcentaje { get; set; }
        public string? Precio2 { get; set; }
        public int? PorcentajeProfit2 { get; set; }
        public string? Precio3 { get; set; }
        public int? PorcentajeProfit3 { get; set; }
        public decimal? Iva { get; set; }
        public int Redondeo { get; set; }
    }
}
