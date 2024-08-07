using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model.Afip.Factura
{
    public class FacturaQR
    {
        public int ver { get; set; }
        public string fecha { get; set; }
        public long cuit { get; set; }
        public int ptoVta { get; set; }
        public int tipoCmp { get; set; }
        public int nroCmp { get; set; }
        public decimal importe { get; set; }
        public string moneda { get; set; }
        public decimal ctz { get; set; }
        public int? tipoDocRec { get; set; }
        public long? nroDocRec { get; set; }
        public string tipoCodAut { get; set; }
        public long codAut { get; set; }
    }
}
