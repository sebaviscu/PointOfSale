using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public partial class ClienteMovimiento
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
        public ClienteMovimiento(int idCliente, decimal total, string registrationUser,int idTienda, int? idSale)
        {
            IdCliente = idCliente;
            IdSale = idSale;
            Total = total;
            IdTienda= idTienda;
            RegistrationDate = DateTimeNowArg;
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
        public TipoMovimientoCliente TipoMovimiento { get; set; }
        public int IdTienda { get; set; }

    }
}
