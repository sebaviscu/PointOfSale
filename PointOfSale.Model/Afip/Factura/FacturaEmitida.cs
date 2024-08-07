using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PointOfSale.Model.Afip.Factura
{
    public class FacturaEmitida
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdFacturaEmitida { get; set; }
        public string? CAE { get; set; }
        public DateTime? CAEVencimiento { get; set; }
        public DateTime FechaEmicion { get; set; }
        public int NroDocumento { get; set; }
        public int TipoDocumentoId { get; set; }
        public string? TipoDocumento { get; set; }
        public string? Resultado { get; set; }
        public string? Errores { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }
        public int? NroFactura { get; set; }
        public int PuntoVenta { get; set; }
        public string? TipoFactura { get; set; }
        public decimal? ImporteTotal { get; set; }
        public decimal ImporteNeto { get; set; }
        public decimal ImporteIVA { get; set; }

        public int? IdSale { get; set; }
        public Sale? Sale { get; set; }

        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        public int IdTienda { get; set; }
        public Tienda? Tienda { get; set; }
    }
}
