using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Utilities
{
    public class TicketModel
    {
        static StringBuilder line = new StringBuilder();

        readonly int MAX = 34;

        public StringBuilder Lineas
        {
            get { return line; }
        }

        public void TextoAgradecimiento(string text)
        {
            var ticket = string.Empty;
            var parte1 = string.Empty;
            var cort = 0;

            var m = text.Length;
            if (m > MAX)
            {
                cort = m - MAX;
                text = text.Remove(MAX, cort);
            }
            else { parte1 = text; }
            m = (int)(MAX - parte1.Length) / 2;

            ticket += new string(' ', m);

            var texto = ticket += parte1 + "\n";
            line.AppendLine(texto);
        }

        public void LineasGuion()
        {
            string LineaGuion = new string('-', MAX);
            line.AppendLine(LineaGuion);
        }
        public void LineasTotal()
        {
            string LineaGuion = new string('=', MAX);
            line.AppendLine(LineaGuion);
        }

        public void TextoIzquierda(string par1)
        {
            var parte1 = string.Empty;
            var ticket = string.Empty;
            var cort = 0;

            var m = par1.Length;
            if (m > MAX)
            {
                cort = m - MAX;
                parte1 = par1.Remove(MAX, cort);
            }
            else { parte1 = par1; }
            var text = ticket = parte1;
            line.AppendLine(text);

        }

        public void TextoCentro(string par1)
        {
            var parte1 = string.Empty;
            var ticket = string.Empty; 
            var cort = 0;

            var m = par1.Length;
            if (m > MAX)
            {
                cort = m - MAX;
                parte1 = par1.Remove(MAX, cort);
            }
            else { parte1 = par1; }
            m = (int)(MAX - parte1.Length) / 2;

            ticket += new string(' ', m);

            var text = ticket += parte1 + "\n";
            line.AppendLine(text);

        }

        public void AgregaTotales(string par1, double total)
        {
            var ticket = string.Empty;
            var parte1 = string.Empty;
            var parte2 = string.Empty;
            var cort = 0;

            var m = par1.Length;
            if (m > 25)
            {
                cort = m - 25;
                parte1 = par1.Remove(25, cort);
            }
            else { parte1 = par1; }
            ticket = parte1;
            parte2 = "$" + total.ToString();
            m = MAX - (parte1.Length + parte2.Length);

            ticket += new string(' ', m);

            var text = ticket += parte2;
            line.AppendLine(text);

        }

        public void AgregaArticulo(string articulo, decimal precio, decimal cant, decimal subtotalDecimal)
        {
            string elementos = string.Empty;
            var nroEspacios = 0;

            if (articulo.Length > MAX)
            {
                articulo = articulo.Substring(0, MAX - 5) + "...";
            }

            line.AppendLine(articulo);

            var precioString = MostrarNumeroConDecimales(precio);
            var cantString = MostrarNumeroConDecimales(cant);
            var subtotal = MostrarNumeroConDecimales(subtotalDecimal);
            elementos += $" {cantString} x ${precioString}";

            //colocar el subtotal a la dercha
            nroEspacios = ((MAX - subtotal.Length) - elementos.Length) - 2;

            elementos += new string(' ', nroEspacios) + "$" + subtotal;
            line.AppendLine(elementos);
        }

        static string MostrarNumeroConDecimales(decimal numero)
        {
            // Verificar si el número tiene decimales
            return numero % 1 == 0 ? Math.Truncate(numero).ToString() : numero.ToString();
        }
    }
}