using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;

namespace PointOfSale.Business.Services
{
    public class TicketService : ITicketService
    {

        public string TicketSale(Sale sale, Tienda tienda)
        {
            return CreateTicket(tienda, sale.RegistrationDate.Value, sale.Total.Value, sale.DetailSales);
        }

        public void ImprimirTiket(string impresora, string line)
        {
            PrinterModel.SendStringToPrinter(impresora, line);
        }


        public string TicketVentaWeb(VentaWeb sale, Tienda tienda)
        {
            return CreateTicket(tienda, sale.RegistrationDate.Value, sale.Total.Value, sale.DetailSales);
        }
        private string CreateTicket(Tienda tienda, DateTime registrationDate, decimal total, ICollection<DetailSale> detailSales)
        {
            if (string.IsNullOrEmpty(tienda.NombreImpresora))
            {
                return string.Empty;
            }

            var Ticket1 = new TicketModel();
            Ticket1.TextoIzquierda("");

            Ticket1.TextoCentro(tienda.Nombre.ToUpper());
            Ticket1.LineasGuion();

            Ticket1.TextoIzquierda("");
            Ticket1.TextoIzquierda("FACTURA ");
            Ticket1.TextoIzquierda("No Fac: 0120102");
            Ticket1.TextoIzquierda("Fecha:" + registrationDate.ToShortDateString() + " " + registrationDate.ToShortTimeString());
            Ticket1.LineasGuion();
            Ticket1.TextoIzquierda("");

            foreach (var d in detailSales)
            {
                Ticket1.AgregaArticulo(d.DescriptionProduct.ToUpper(),
                   d.Price.Value,
                   d.Quantity.Value,
                   d.Total.Value);
            }

            Ticket1.TextoIzquierda(" ");
            Ticket1.TextoIzquierda(" ");
            Ticket1.LineasTotal();
            Ticket1.AgregaTotales("Total", double.Parse(total.ToString()));
            Ticket1.LineasTotal();
            Ticket1.TextoIzquierda(" ");

            Ticket1.TextoIzquierda(" ");
            Ticket1.TextoAgradecimiento("Gracias por su compra!");
            Ticket1.TextoIzquierda(" ");

            ImprimirTiket(tienda.NombreImpresora, Ticket1.Lineas.ToString());
            return Ticket1.Lineas.ToString();
        }
    }
}
