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

        public StringBuilder CrearLineas(Sale sale)
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
            TicketModel.LineasGuion();

            TicketModel.EncabezadoVenta();
            TicketModel.LineasGuion();

            foreach (var d in sale.DetailSales)
            {
                Ticket1.AgregaArticulo(d.DescriptionProduct.ToString(), 
                    double.Parse(d.Price.Value.ToString()), 
                    int.Parse(d.Quantity.Value.ToString()), 
                    double.Parse(d.Total.ToString())); //imprime una linea de descripcion
            }


            TicketModel.LineasGuion();
            Ticket1.TextoIzquierda(" ");
            Ticket1.AgregaTotales("Total", double.Parse("100")); // imprime linea con total
            Ticket1.TextoIzquierda(" ");
            Ticket1.AgregaTotales("Efectivo Entregado:", double.Parse("150"));
            Ticket1.AgregaTotales("Efectivo Devuelto:", double.Parse("50"));


            // Ticket1.LineasTotales(); // imprime linea 

            Ticket1.TextoIzquierda(" ");
            Ticket1.TextoCentro("**********************************");
            Ticket1.TextoCentro("*     Gracias por preferirnos    *");

            Ticket1.TextoCentro("**********************************");
            Ticket1.TextoIzquierda(" ");

            return Ticket1.Lineas;
        }

        public void ImprimirTiket(string stringimpresora, StringBuilder line)
        {
            PrinterModel.SendStringToPrinter(stringimpresora, line.ToString());
            line = new StringBuilder();
        }
    }
}
