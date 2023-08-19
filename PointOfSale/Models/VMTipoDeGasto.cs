using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMTipoDeGasto
    {
        public int IdTipoGastos { get; set; }
        public string GastoParticular { get; set; }
        public string Descripcion { get; set; }

        public ICollection<Gastos>? Gastos { get; set; }
    }
}
