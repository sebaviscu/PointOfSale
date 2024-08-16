using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Utilities
{
    public class TicketModel
    {
        readonly int MAX = 26;
        readonly int Margin = 2; // Margen variable global

        public TicketModel()
        {
            line = new StringBuilder();
            ImagesTicket = new List<Images>();
        }

        #region Propiedades
        static StringBuilder line = new StringBuilder();

        public List<Images> ImagesTicket { get; set; }

        #endregion

        public string Ticket => Lineas.ToString();

        public StringBuilder Lineas => line;

        public void TextoAgradecimiento(string text)
        {
            string texto = FormatearTextoCentro(text);
            AppendLineWithMargin(texto);
        }

        public void LineasGuion() => AppendLineWithMargin(new string('-', MAX));

        public void LineasTotal() => AppendLineWithMargin(new string('=', MAX));

        public void TextoIzquierda(string text)
        {
            string texto = CortarTextoMax(text, MAX - Margin);
            AppendLineWithMargin(texto);
        }
        public void InsertarImagen(string flag, string imageBase64)
        {
            line.AppendLine("");
            line.AppendLine($"[[{flag}]]");
            ImagesTicket.Add(new Images(imageBase64, flag));
        }

        public void TextoCentro(string text)
        {
            string texto = FormatearTextoCentro(text);
            AppendLineWithMargin(texto);
        }

        public void TextoBetween(string text1, string text2)
        {
            string textoFormateado = FormatearTextoBetween(text1, text2);
            AppendLineWithMargin(textoFormateado);
        }

        public void AgregaTotales(string text, double total)
        {
            string totalString = $"${total}";
            string textoFormateado = FormatearTextoBetween(text, totalString);
            AppendLineWithMargin(textoFormateado);
        }

        public void AgregaArticulo(string articulo, decimal precio, decimal cant, decimal subtotalDecimal, decimal? iva)
        {
            string articuloCortado = CortarTextoMax(articulo, MAX - 5) + (articulo.Length > MAX - 5 ? "..." : "");
            AppendLineWithMargin(articuloCortado);

            string elementos = $" {MostrarNumeroConDecimales(cant)} x ${MostrarNumeroConDecimales(precio)}";
            if (iva != null && iva > 0)
            {
                elementos += $" ({iva}%)";
            }

            string subtotal = $"${MostrarNumeroConDecimales(subtotalDecimal)}";
            int nroEspacios = MAX - elementos.Length - subtotal.Length;

            string lineaArticulo = elementos + new string(' ', nroEspacios) + subtotal;
            AppendLineWithMargin(lineaArticulo);
        }

        private string FormatearTextoCentro(string text)
        {
            text = CortarTextoMax(text, MAX - 2 * Margin);
            int espacioIzq = (MAX - text.Length) / 2;
            return new string(' ', espacioIzq) + text;
        }

        private string FormatearTextoBetween(string text1, string text2)
        {
            text1 = CortarTextoMax(text1, MAX / 2 - Margin);
            text2 = CortarTextoMax(text2, MAX / 2 - Margin);

            int espaciosEntre = MAX - text1.Length - text2.Length - Margin;
            return text1 + new string(' ', espaciosEntre) + text2;
        }

        private string CortarTextoMax(string texto, int maxLen)
        {
            return texto.Length > maxLen ? texto.Substring(0, maxLen) : texto;
        }

        private void AppendLineWithMargin(string text)
        {
            // Aplica el margen al principio de cada línea
            string textoConMargen = new string(' ', Margin / 2) + text;
            line.AppendLine(textoConMargen);
        }

        static string MostrarNumeroConDecimales(decimal numero)
        {
            return numero % 1 == 0 ? Math.Truncate(numero).ToString() : numero.ToString();
        }
    }


    public class Images
    {
        public Images(string imageBase64, string flag)
        {
            ImageBase64 = imageBase64;
            Flag = $"[[{flag}]]";
        }

        public string ImageBase64 { get; set; }
        public string Flag { get; set; }
    }
}