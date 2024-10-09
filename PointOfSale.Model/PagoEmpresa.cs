using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class PagoEmpresa : EntityBase
    {
        public int IdEmpresa { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Importe { get; set; }
        public string? Comentario { get; set; }
        public EstadoPago EstadoPago { get; set; }

        public decimal? ImporteSinIva { get; set; }
        public decimal? Iva { get; set; }
        public decimal? IvaImporte { get; set; }
        public string? NroFactura { get; set; }
        public string? TipoFactura { get; set; }
        public bool? FacturaPendiente { get; set; }


        public Empresa? Empresa { get; set; }
    }
}
