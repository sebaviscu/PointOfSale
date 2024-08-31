using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class MovimientoCaja
    {
        public int IdMovimientoCaja { get; set; }
        public string Comentario { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string RegistrationUser { get; set; }
        public decimal Importe { get; set; }
        public int IdRazonMovimientoCaja { get; set; }
        public int IdTienda { get; set; }
        public int IdTurno { get; set; }
        public virtual RazonMovimientoCaja? RazonMovimientoCaja { get; set; }
    }

}
