using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model.Afip.Factura
{
    public class DetalleFacturaIva
    {
        public DetalleFacturaIva(ImporteIva importeIva)
        {
            TipoIva = importeIva.TipoIva;
            ImporteIVA = importeIva.ImporteIVA;
            ImporteNeto = importeIva.ImporteNeto;
            ImporteTotal = importeIva.ImporteTotal;
        }
        public DetalleFacturaIva()
        {
            
        }
        public int Id { get; set; }
        public IVA_Afip TipoIva { get; set; }
        public double ImporteNeto { get; set; }
        public double ImporteIVA { get; set; }
        public double ImporteTotal { get; set; }
        public int IdFacturaEmitida { get; set; }
        public FacturaEmitida? FacturaEmitida { get; set; }

    }
}
