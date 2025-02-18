using Microsoft.EntityFrameworkCore.Diagnostics;
using PointOfSale.Model.Afip.Factura;
using System;
using System.Collections.Generic;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public partial class Sale
    {
        public int IdSale { get; set; }
        public string? SaleNumber { get; set; }
        public int? IdTypeDocumentSale { get; set; }
        public string? CustomerDocument { get; set; }
        public string? ClientName { get; set; }
        public decimal? Total { get; set; }
        public decimal? TotalSinComision => Total * 1 - ((TypeDocumentSaleNavigation != null ? TypeDocumentSaleNavigation.Comision : 0) / 100);
        public DateTime? RegistrationDate { get; set; }
        public string RegistrationUser { get; set; }

        public virtual TypeDocumentSale? TypeDocumentSaleNavigation { get; set; }
        public virtual ICollection<DetailSale> DetailSales { get; set; } = new HashSet<DetailSale>();
        public int IdTurno { get; set; }
        public Turno Turno { get; set; }
        public int? IdClienteMovimiento { get; set; }
        public ClienteMovimiento? ClienteMovimiento { get; set; }
        public int IdTienda { get; set; }
        public Tienda Tienda { get; set; }
        public decimal? DescuentoRecargo { get; set; }
        public int? IdFacturaEmitida { get; set; }
        public virtual ICollection<FacturaEmitida>? FacturasEmitidas { get; set; }
        public bool? IsWeb { get; set; }
        public string? Observaciones { get; set; }
        public bool IsDelete { get; set; } = false;
        public bool? ResultadoFacturacion {  get; set; }
        public TipoFactura? TipoFactura { get; set; }
    }
}
