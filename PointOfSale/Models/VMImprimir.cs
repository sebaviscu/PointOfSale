﻿using PointOfSale.Business.Utilities;

namespace PointOfSale.Models
{
    public class VMImprimir
    {
        public string? NombreImpresora { get; set; }
        public string? Ticket { get; set; }
        public string? urlQr { get; set; }
        public List<Images> ImagesTicket { get; set; }
    }
}