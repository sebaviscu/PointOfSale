using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model.Afip.Factura
{
    public class FacturaEmitida
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdFacturaEmitida { get; set; }
        public string? CAE { get; set; }
        public DateTime CAEVencimiento { get; set; }
        public DateTime FechaEmicion { get; set; }
        public int NroDocumento { get; set; }
        public int TipoDocumentoId { get; set; }
        public string? TipoDocumento { get; set; }
        public string? Resultado { get; set; }
        public string? Errores { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }

        public int? IdSale { get; set; }
        public Sale? Sale { get; set; }

        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

    }
}
