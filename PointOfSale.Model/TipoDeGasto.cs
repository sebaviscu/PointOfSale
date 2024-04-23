using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class TipoDeGasto
    {
        public int IdTipoGastos { get; set; }
        public TipoDeGastoEnum GastoParticular { get; set; }
        public string Descripcion { get; set; }
        public decimal? Iva { get; set; }
        public TipoFactura? TipoFactura { get; set; }

        public ICollection<Gastos>? Gastos { get; set; }
    }
}
