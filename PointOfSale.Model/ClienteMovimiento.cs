using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public partial class ClienteMovimiento
    {
        public ClienteMovimiento(int idCliente, decimal total, string registrationUser, int? idSale)
        {
            IdCliente = idCliente;
            IdSale = idSale;
            Total = total;
            RegistrationDate = DateTime.Now;
            RegistrationUser = registrationUser;
        }

        public int IdClienteMovimiento { get; set; }
        public int IdCliente { get; set; }
        public int? IdSale { get; set; }
        public decimal Total { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string RegistrationUser { get; set; }
        public Cliente Cliente { get; set; }
        public Sale? Sale { get; set; }

    }
}
