using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model.Auditoria
{
    public class ATipoDocumentoVenta
    {
        public ATipoDocumentoVenta(TypeDocumentSale typeDocumentSale)
        {
            Description = typeDocumentSale.Description;
            IsActive = typeDocumentSale.IsActive;
            Invoice = typeDocumentSale.Invoice;
            Web = typeDocumentSale.Web;
        }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool Invoice { get; set; }
        public bool Web { get; set; }
    }
}
