using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model.Afip.Factura
{
    public class ImporteIva
    {
        public IVA_Afip TipoIva { get; set; }
        public double ImporteNeto { get; set; }
        public double ImporteIVA { get; set; }
        public double ImporteTotal => ImporteNeto + ImporteIVA;
    }

    public enum IVA_Afip
    {
        IVA_0 = 3,
        IVA_105 = 4,
        IVA_21 = 5,
        IVA_27 = 6,
        IVA_05 = 8,
        IVA_025 = 9
    }
}
