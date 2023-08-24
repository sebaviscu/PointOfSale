using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class TicketService : ITicketService
    {

        public bool ImprimirTicket(Sale sale)
        {
            var Ticket1 = new TicketModel();

            Ticket1.TextoCentro("Empresa xxxxx "); //imprime una linea de descripcion
            Ticket1.TextoCentro("**********************************");

            Ticket1.TextoIzquierda("");
            Ticket1.TextoCentro("Factura de Venta"); //imprime una linea de descripcion
            Ticket1.TextoIzquierda("No Fac: 0120102");
            Ticket1.TextoIzquierda("Fecha:" + DateTime.Now.ToShortDateString() + " Hora:" + DateTime.Now.ToShortTimeString());
            Ticket1.TextoIzquierda("Le Atendio: xxxx");
            Ticket1.TextoIzquierda("");
            Ticket1.LineasGuion();
            Ticket1.TextoIzquierda("");


            foreach (var d in sale.DetailSales)
            {
                Ticket1.AgregaArticulo(d.DescriptionProduct.ToUpper(),
                   d.Price.Value,
                   d.Quantity.Value,
                   d.Total.Value); //imprime una linea de descripcion
            }


            Ticket1.TextoIzquierda(" ");
            Ticket1.TextoIzquierda(" ");
            Ticket1.LineasTotal();
            Ticket1.AgregaTotales("Total", double.Parse(sale.Total.ToString())); // imprime linea con total
            Ticket1.LineasTotal();
            Ticket1.TextoIzquierda(" ");


            // Ticket1.LineasTotales(); // imprime linea 

            //Ticket1.TextoIzquierda(" ");
            //Ticket1.TextoCentro("**********************************");
            //Ticket1.TextoCentro("*     Gracias por preferirnos    *");

            //Ticket1.TextoCentro("**********************************");
            Ticket1.TextoIzquierda(" ");

            return ImprimirTiket("Microsoft XPS Document Writer", Ticket1.Lineas);
        }

        private bool ImprimirTiket(string stringimpresora, StringBuilder line)
        {
            return PrinterModel.SendStringToPrinter(stringimpresora, line.ToString());
        }
    }
}
