﻿using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMTypeDocumentSale
    {
        public int IdTypeDocumentSale { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public bool? Web { get; set; }
        public TipoFactura? TipoFactura { get; set; }
        public string? TipoFacturaString { get; set; }
        public decimal? Comision { get; set; }
        public int? DescuentoRecargo { get; set; }

    }
}
