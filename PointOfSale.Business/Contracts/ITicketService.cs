using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Business.Contracts
{
    public interface ITicketService
    {
        Task<TicketModel> TicketSale(Sale sale, Ajustes ajustes, FacturaEmitida? facturaEmitida);
        Task<TicketModel> TicketVentaWeb(VentaWeb sale, Ajustes ajustes, FacturaEmitida? facturaEmitida);
    }
}
