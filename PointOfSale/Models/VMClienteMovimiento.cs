using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMClienteMovimiento
    {

        public int IdClienteMovimiento { get; set; }
        public int IdCliente { get; set; }
        public int? IdSale { get; set; }
        public decimal Total { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string RegistrationUser { get; set; }
        public Cliente Cliente { get; set; }
        public Sale? Sale { get; set; }
        public string TotalString { get; set; }
    }
}
