using System;
using System.Collections.Generic;

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
        public bool Invoice { get; set; }
        public bool Web { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public virtual ICollection<Sale> Sales { get; set; }
        public virtual ICollection<VentaWeb> VentasWeb { get; set; }
    }
}
