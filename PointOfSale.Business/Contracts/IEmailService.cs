using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IEmailService
    {
        Task NotificarCierreCaja(int idTienda);

        Task EnviarTicketEmail(int idTienda, string emailReceptor, byte[] attachment);
    }
}
