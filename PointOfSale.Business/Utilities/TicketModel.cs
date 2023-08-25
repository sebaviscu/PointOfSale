using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Utilities
{
    public class TicketModel
    {
        public TicketModel()
        {
            line = new StringBuilder();
        }
        public static StringBuilder line = new StringBuilder();
        string ticket = "";
        string parte1, parte2;

        int MAX = 30 - 4;
        int cort;

        public StringBuilder Lineas
        {
            get { return line; }
        }

        public void TextoAgradecimiento(string text)
        {
            ticket = "";
            var m = text.Length;
            if (m > MAX)
            {
                cort = m - MAX;
                text = text.Remove(MAX, cort);
            }
            else { parte1 = text; }
            m = (int)(MAX - parte1.Length) / 2;
            for (int i = 0; i < m; i++)
            {
                ticket += " ";
            }
            var texto = ticket += parte1 + "\n";
            line.AppendLine("  " + texto);
        }

        public void LineasGuion()
        {
            string LineaGuion = string.Empty;

            for (int i = 0; i < MAX; i++)
            {
                LineaGuion += "-";
            }

            line.AppendLine("  " + LineaGuion);
        }
        public void LineasTotal()
        {
            string LineaGuion = string.Empty;

            for (int i = 0; i < MAX; i++)
            {
                LineaGuion += "=";
            }

            line.AppendLine("  " + LineaGuion);
        }

        public void TextoIzquierda(string par1)
        {
            var m = par1.Length;
            if (m > MAX)
            {
                cort = m - MAX;
                parte1 = par1.Remove(MAX, cort);
            }
            else { parte1 = par1; }
            var text = ticket = parte1;
            line.AppendLine("  " + text);

        }
        public void TextoDerecha(string par1)
        {
            ticket = "";
            var m = par1.Length;
            if (m > MAX)
            {
                cort = MAX - m;
                parte1 = par1.Remove(m, cort);
            }
            else { parte1 = par1; }
            m = MAX - par1.Length;
            for (int i = 0; i < m; i++)
            {
                ticket += " ";
            }
            var text = ticket += parte1 + "\n";
            line.AppendLine("  " + text);

        }
        public void TextoCentro(string par1)
        {
            ticket = "";
            var m = par1.Length;
            if (m > MAX)
            {
                cort = m - MAX;
                parte1 = par1.Remove(MAX, cort);
            }
            else { parte1 = par1; }
            m = (int)(MAX - parte1.Length) / 2;
            for (int i = 0; i < m; i++)
            {
                ticket += " ";
            }
            var text = ticket += parte1 + "\n";
            line.AppendLine("  " + text);

        }
        public void TextoExtremos(string par1, string par2)
        {
            var m = par1.Length;
            if (m > 18)
            {
                cort = m - 18;
                parte1 = par1.Remove(18, cort);
            }
            else { parte1 = par1; }
            ticket = parte1;
            m = par2.Length;
            if (m > 18)
            {
                cort = m - 18;
                parte2 = par2.Remove(18, cort);
            }
            else { parte2 = par2; }
            m = MAX - (parte1.Length + parte2.Length);
            for (int i = 0; i < m; i++)
            {
                ticket += " ";
            }
            var text = ticket += parte2 + "\n";
            line.AppendLine("  " + text);

        }
        public void AgregaTotales(string par1, double total)
        {
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
            for (int i = 0; i < m; i++)
            {
                ticket += " ";
            }
            var text = ticket += parte2;
            line.AppendLine("  " + text);

        }

        // se le pasan los Aticulos  con sus detalles
        public void AgregaArticulo(string Articulo, decimal precio, decimal cant, decimal subtotal)
        {
            string elementos = "", espacios = "";
            var nroEspacios = 0;

            if (Articulo.Length > MAX)
            {
                //cort = max - 16;
                Articulo = Articulo.Substring(0, MAX - 7) + "...";
            }

            for (int i = 0; i < (MAX - Articulo.Length); i++)
            {
                espacios += " ";

            }
            elementos = Articulo + espacios;
            line.AppendLine("  " + elementos);

            elementos = string.Empty;

            nroEspacios = (7 - cant.ToString().Length);
            espacios = "";
            for (int i = 0; i < nroEspacios; i++)
            {
                espacios += " ";
            }
            elementos += espacios + cant.ToString() + " x $" + precio.ToString();

            //colocar el subtotal a la dercha
            nroEspacios = ((MAX - subtotal.ToString().Length) - elementos.Length) - 1;
            espacios = "";

            for (int i = 0; i < nroEspacios; i++)
            {
                espacios += " ";
            }
            elementos += espacios + "$" + subtotal.ToString();
            line.AppendLine("  " + elementos);
        }
    }
}