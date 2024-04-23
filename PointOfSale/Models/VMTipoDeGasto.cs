using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMTipoDeGasto
    {
        public int IdTipoGastos { get; set; }
        public string GastoParticular { get; set; }
        public string Descripcion { get; set; }
        public decimal? Iva { get; set; }
        public TipoFactura? TipoFactura { get; set; }
        public ICollection<Gastos>? Gastos { get; set; }
    }
}
