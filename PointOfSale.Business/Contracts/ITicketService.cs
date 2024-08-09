using PointOfSale.Model;

namespace PointOfSale.Business.Contracts
{
    public interface ITicketService
    {
        string TicketSale(Sale sale, Ajustes ajustes);
        string TicketVentaWeb(VentaWeb sale, Ajustes ajustes);
    }
}
