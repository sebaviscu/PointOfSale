using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMRazonMovimientoCaja
    {
        public int IdRazonMovimientoCaja { get; set; }
        public string Descripcion { get; set; }
        public TipoMovimientoCaja Tipo { get; set; }
        public string TipoString => Tipo.ToString();

        public bool Estado { get; set; }
        //public virtual ICollection<MovimientoCaja>? MovimientoCajas { get; set; }
    }
}
