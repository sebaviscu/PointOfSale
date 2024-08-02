using Org.BouncyCastle.Tls;
using System.Text.RegularExpressions;

namespace PointOfSale.Models
{
    public class VMX509Certificate2
    {
        public string Subject { get; set; }
        public DateTime NotBefore { get; set; }
        public DateTime NotAfter { get; set; }
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