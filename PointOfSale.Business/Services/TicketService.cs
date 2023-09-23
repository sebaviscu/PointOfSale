using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;

namespace PointOfSale.Business.Services
{
    public class TicketService : ITicketService
    {

        public string ImprimirTicket(Sale sale, Tienda tienda)
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
            Ticket1.TextoIzquierda("Fecha:" + sale.RegistrationDate.Value.ToShortDateString() + " " + sale.RegistrationDate.Value.ToShortTimeString());
            Ticket1.LineasGuion();
            Ticket1.TextoIzquierda("");

            foreach (var d in sale.DetailSales)
            {
                Ticket1.AgregaArticulo(d.DescriptionProduct.ToUpper(),
                   d.Price.Value,
                   d.Quantity.Value,
                   d.Total.Value);
            }

            Ticket1.TextoIzquierda(" ");
            Ticket1.TextoIzquierda(" ");
            Ticket1.LineasTotal();
            Ticket1.AgregaTotales("Total", double.Parse(sale.Total.ToString()));
            Ticket1.LineasTotal();
            Ticket1.TextoIzquierda(" ");

            Ticket1.TextoIzquierda(" ");
            Ticket1.TextoAgradecimiento("Gracias por su compra!");
            Ticket1.TextoIzquierda(" ");

            ImprimirTiket(tienda.NombreImpresora, Ticket1.Lineas.ToString());
            return Ticket1.Lineas.ToString();
        }

        public void ImprimirTiket(string impresora, string line)
        {
            PrinterModel.SendStringToPrinter(impresora, line);
        }
    }
}
