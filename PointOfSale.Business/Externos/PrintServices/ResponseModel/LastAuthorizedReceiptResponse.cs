﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PointOfSale.Business.Externos.PrintServices.ResponseModel
{
    public class LastAuthorizedReceiptResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("numeroComprobante")]
        public int NumeroComprobante { get; set; }
    }
}
