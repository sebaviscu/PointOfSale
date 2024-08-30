using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class RazonMovimientoCaja
    {
        public int IdRazonMovimientoCaja { get; set; }
        public string Descripcion { get; set; }
        public TipoMovimientoCaja Tipo { get; set; }
        public virtual ICollection<MovimientoCaja>? MovimientoCajas { get; set; }
        public bool Estado { get; set; }
    }
}
