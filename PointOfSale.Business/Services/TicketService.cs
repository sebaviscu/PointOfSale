using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using System.Drawing.Printing;

namespace PointOfSale.Business.Services
{
    public class TicketService : ITicketService
    {

        public string TicketSale(Sale sale, Tienda tienda)
        {
            return CreateTicket(tienda, sale.RegistrationDate.Value, sale.Total.Value, sale.DetailSales, sale.DescuentoRecargo);
        }

        public string TicketVentaWeb(VentaWeb sale, Tienda tienda)
        {
            return CreateTicket(tienda, sale.RegistrationDate.Value, sale.Total.Value, sale.DetailSales, null);
        }
        public void ImprimirTiket(string impresora, string line)
        {
            PrinterModel.SendStringToPrinter(impresora, line);
        }

        private string CreateTicket(Tienda tienda, DateTime registrationDate, decimal total, ICollection<DetailSale> detailSales, decimal? descuentoRecargo)
        {

            if (string.IsNullOrEmpty(tienda.NombreImpresora))
            {
                return string.Empty;
            }

            var Ticket1 = new TicketModel();
            Ticket1.TextoIzquierda("");

            Ticket1.TextoCentro(tienda.Nombre.ToUpper());

            Ticket1.LineasGuion();
            Ticket1.TextoIzquierda("No Fac: 0120102");
            Ticket1.TextoIzquierda("Fecha: " + registrationDate.ToShortDateString() + " " + registrationDate.ToShortTimeString());
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

            if (descuentoRecargo != null && descuentoRecargo != 0)
            {
                if (descuentoRecargo > 0)
                {
                    Ticket1.TextoIzquierda(" Regargo: $" + descuentoRecargo);
                }
                else
                {
                    Ticket1.TextoIzquierda(" Descuento: $" + (descuentoRecargo * -1));
                }
                Ticket1.TextoIzquierda(" ");
            }

            Ticket1.LineasTotal();
            Ticket1.AgregaTotales("Total", double.Parse(total.ToString()));
            Ticket1.LineasTotal();

            Ticket1.TextoIzquierda(" ");
            Ticket1.TextoAgradecimiento("¡Gracias por su compra!");
            Ticket1.TextoIzquierda(" ");
            Ticket1.TextoIzquierda(" ");

            return Ticket1.Lineas.ToString();
        }
    }
}
