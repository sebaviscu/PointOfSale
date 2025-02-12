using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Externos.PrintServices.ResponseModel
{
    public class LastAuthorizedReceiptResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }

        public int NumeroComprobante { get; set; }
    }
}
