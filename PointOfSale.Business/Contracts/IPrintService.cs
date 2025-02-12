using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointOfSale.Business.Externos.PrintServices.ResponseModel;

namespace PointOfSale.Business.Contracts
{
    public interface IPrintService
    {

        Task<int> GetLastAuthorizedReceiptAsync(int ptoVenta, int idTipoComprobante);

    }
}
