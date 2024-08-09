using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PointOfSale.Model.Afip.Factura
{
    public class VMX509Certificate2
    {
        public string Subject { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaCaducidad { get; set; }
        public string Cuil => ExtractCUIT();

        string ExtractCUIT()
        {
            string pattern = @"SERIALNUMBER=CUIT (\d{11})";
            Match match = Regex.Match(Subject, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
    }
}
