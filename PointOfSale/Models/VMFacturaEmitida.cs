using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMFacturaEmitida
    {
        public int IdFacturaEmitida { get; set; }
        public string? CAE { get; set; }
        public DateTime? CAEVencimiento { get; set; }
        public DateTime? FechaEmicion { get; set; }
        public long? NroDocumento { get; set; }
        public int? TipoDocumentoId { get; set; }
        public string? TipoDocumento { get; set; }
        public string? Resultado { get; set; }
        public string? Observaciones { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }
        public int? NroFactura { get; set; }
        public int? PuntoVenta { get; set; }
        public string? NroFacturaString { get; set; }
        public string? PuntoVentaString { get; set; }
        public string? TipoFactura { get; set; }
        public decimal? ImporteTotal { get; set; }
        public decimal? ImporteNeto { get; set; }
        public decimal? ImporteIVA { get; set; }
        public string? QR { get; set; }
        public int? IdFacturaAnulada { get; set; }
        public string? FacturaAnulada { get; set; }

        public int? IdSale { get; set; }
        public VMSale? Sale { get; set; }

        public int? IdCliente { get; set; }
        public VMCliente? Cliente { get; set; }

        public int? IdTienda { get; set; }
        public VMTienda? Tienda { get; set; }

        public string NumeroFacturaString => NroFactura.HasValue && NroFactura.Value != 0 && PuntoVenta != 0 ? $"{PuntoVenta.Value.ToString("D4")}-{NroFactura.Value.ToString("D8")}" : "";

    }
}
