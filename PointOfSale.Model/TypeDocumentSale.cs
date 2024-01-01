using System;
using System.Collections.Generic;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public partial class TypeDocumentSale
    {
        public TypeDocumentSale()
        {
            Sales = new HashSet<Sale>();
        }

        public int IdTypeDocumentSale { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool Web { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public TipoFactura TipoFactura { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
        public virtual ICollection<VentaWeb> VentasWeb { get; set; }
    }
}
