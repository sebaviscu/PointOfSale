using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model.Input
{
    public class RegistrationSaleInput
    {
        public List<MultiplesFormaPago> MultiplesFormaDePago { get; set; }
        public int? ClientId { get; set; }
        public TipoMovimientoCliente? TipoMovimiento { get; set; }
        public string? CuilFactura { get; set; }
        public int? IdClienteFactura { get; set; }
        public bool ImprimirTicket { get; set; }
    }

    public class MultiplesFormaPago
    {
        public decimal Total { get; set; }
        public int? FormaDePago { get; set; }

    }

    public class SaleResult
    {

    }
}
