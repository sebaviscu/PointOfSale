using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Business.Contracts
{
    public interface ITicketService
    {
        Task<TicketModel> TicketSale(Sale sale, Ajustes ajustes, FacturaEmitida? facturaEmitida);
        Task<TicketModel> TicketSale(VentaWeb sale, Ajustes ajustes);

        byte[] PdfTicket(string ticket, List<Images> ImagesTicket);

        Task<TicketModel> CierreTurno(Turno turno, Dictionary<string, decimal> VentasRegistradas);
    }
}
