using System.Text;

namespace PointOfSale.Business.Utilities
{
    public class TicketModel
    {
        readonly int MAX = 26;

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
            line.AppendLine(text);
        }

        public void LineasGuion() => line.AppendLine(new string('-', MAX));

        public void LineasTotal() => line.AppendLine(new string ('=', MAX));

        public void TextoIzquierda(string text)
        {
            string textoFormateado = CortarTextoMax(text, MAX);
            line.AppendLine(textoFormateado);
        }
        public void InsertarImagen(string flag, string imageBase64)
        {
            line.AppendLine("");
            line.AppendLine($"[[{flag}]]");
            ImagesTicket.Add(new Images(imageBase64, flag));
        }

        public void TextoCentro(string text)
        {
            string textoFormateado = FormatearTextoCentro(text);
            line.AppendLine(textoFormateado);
        }

        public void TextoBetween(string text1, string text2)
        {
            string textoFormateado = FormatearTextoBetween(text1, text2);
            line.AppendLine(textoFormateado);
        }

        public void AgregaTotales(string text, double total)
        {
            string totalString = $"${total}";
            string textoFormateado = FormatearTextoBetween(text, totalString);
            line.AppendLine(textoFormateado);
        }

        public void AgregaArticulo(string articulo, decimal precio, decimal cant, decimal subtotalDecimal, decimal? iva)
        {
            string articuloCortado = CortarTextoMax(articulo, MAX - 5) + (articulo.Length > MAX - 5 ? "..." : "");
            line.AppendLine(articuloCortado);

            string elementos = $" {MostrarNumeroConDecimales(cant)} x ${MostrarNumeroConDecimales(precio)}";
            if (iva != null && iva > 0)
            {
                elementos += $" ({iva}%)";
            }

            string subtotal = $"${MostrarNumeroConDecimales(subtotalDecimal)}";
            int nroEspacios = MAX - elementos.Length - subtotal.Length;

            string lineaArticulo = elementos + new string(' ', nroEspacios) + subtotal;
            line.AppendLine(lineaArticulo);
        }

        private string FormatearTextoCentro(string text)
        {
            text = CortarTextoMax(text, MAX);
            int espacioIzq = (MAX - text.Length) / 2;
            return new string(' ', espacioIzq) + text;
        }

        public string FormatearTextoBetween(string text1, string text2)
        {
            text1 = CortarTextoMax(text1, MAX);
            text2 = CortarTextoMax(text2, MAX);

            int espaciosEntre = MAX - text1.Length - text2.Length;
            return text1 + new string(' ', espaciosEntre) + text2;
        }

        private string CortarTextoMax(string texto, int maxLen)
        {
            return texto.Length > maxLen ? texto.Substring(0, maxLen) : texto;
        }

        static string MostrarNumeroConDecimales(decimal numero)
        {
            return numero % 1 == 0 ? Math.Truncate(numero).ToString() : numero.ToString();
        }

        public void AgregaVentasCerrarTurno(string metodoPago, decimal valor)
        {
            string articuloCortado = CortarTextoMax(metodoPago, MAX - 10);

            int nroEspacios = MAX - articuloCortado.Length - valor.ToString().Length;

            string lineaArticulo = articuloCortado + new string(' ', nroEspacios) + "$" + valor.ToString();
            line.AppendLine(lineaArticulo);
        }

        public void BetweenCierreTurno(string texto, int valor)
        {
            string articuloCortado = CortarTextoMax(texto, MAX - 10);

            int nroEspacios = MAX - articuloCortado.Length - valor.ToString().Length;

            string lineaArticulo = articuloCortado + new string(' ', nroEspacios) + "$" + valor.ToString();
            line.AppendLine(lineaArticulo);
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