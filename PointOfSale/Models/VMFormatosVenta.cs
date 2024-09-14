using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMFormatosVenta
    {
        public int IdFormatosVenta { get; set; }
        public string Formato { get; set; }
        public double Valor { get; set; }
        public bool Estado { get; set; }
    }
}
