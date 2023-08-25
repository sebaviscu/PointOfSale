using AFIP.Facturacion.Model;
using PointOfSale.Model;
using PointOfSale.Model.Factura;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface ITicketService
    {
        string ImprimirTicket(Sale sale, Tienda tienda);
        void ImprimirTiket(string line);
    }
}
