using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AFIP.Facturacion.Model;
using AfipServiceReference;

namespace PointOfSale.Business.Externos.PrintServices.ResponseModel
{
    public class InvoiceResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("invoice")]
        public FacturacionResponse Invoice { get; set; }
    }
}
