using PointOfSale.Model;

namespace PointOfSale.Business.Contracts
{
    public interface ITicketService
    {
        string ImprimirTicket(Sale sale, Tienda tienda);
        void ImprimirTiket(string impresora, string line);
    }
}
