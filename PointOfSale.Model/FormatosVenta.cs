using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class FormatosVenta
    {
        public int id { get; set; }
        public string Formato { get; set; }
        public double Valor { get; set; }
        public bool Estado { get; set; }
    }
}
