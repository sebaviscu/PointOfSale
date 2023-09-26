using PointOfSale.Model;

namespace PointOfSale.Business.Contracts
{
    public interface ITicketService
    {
        string TicketSale(Sale sale, Tienda tienda);
        string TicketVentaWeb(VentaWeb sale, Tienda tienda);
        void ImprimirTiket(string impresora, string line);
    }
}
