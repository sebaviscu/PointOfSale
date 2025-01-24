using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ganss.Xss;

namespace PointOfSale.Business.Utilities
{
    public class RichTextHelper
    {
        public static string SanitizeHtml(string? inputHtml)
        {
            if (string.IsNullOrWhiteSpace(inputHtml))
                return string.Empty;

            if (inputHtml.Length > 50000)
            {
                throw new ArgumentException("El contenido de 'Sobre Nosotros' demasiado largo.");
            }

            var sanitizer = new HtmlSanitizer();

            // Opcional: Configurar etiquetas y atributos permitidos
            sanitizer.AllowedTags.Add("iframe"); // Si necesitas permitir iframes, por ejemplo
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Add("style");

            // Sanitizar el HTML de entrada
            string sanitizedHtml = sanitizer.Sanitize(inputHtml);

            return sanitizedHtml;
        }
    }
}
