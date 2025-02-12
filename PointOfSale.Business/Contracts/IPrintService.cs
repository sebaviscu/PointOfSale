using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFIP.Facturacion.Model;
using AfipServiceReference;
using PointOfSale.Business.Externos.PrintServices.ResponseModel;
using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Business.Contracts
{
    public interface IPrintService
    {

        Task<int> GetLastAuthorizedReceiptAsync(int ptoVenta, int idTipoComprobante);
        Task<FacturacionResponse> FacturarAsync(FacturaAFIP factura);

        Task<bool> GetHealthcheckAsync();
        Task PrintTicketAsync(string text, string printerName, string[] imagesTicket);
        Task<List<string>> GetPrintersAsync();

    }
}
