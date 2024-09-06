using AFIP.Facturacion.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model.Afip.Factura
{
    public class ComprobanteAsociado
    {
        public int PuntoVenta { get; set; }
        public int TipoComprobante { get; set; }
        public int NroComprobante { get; set; }
        public DateTime? FechaComprobante { get; set; }
        public double? Cuil { get; set; }
    }
}
