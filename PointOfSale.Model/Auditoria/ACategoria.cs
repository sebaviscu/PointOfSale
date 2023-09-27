using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model.Auditoria
{
    public class ACategoria
    {
        public ACategoria(Category category)
        {
            Description = category.Description;
            IsActive= category.IsActive;
        }

        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
