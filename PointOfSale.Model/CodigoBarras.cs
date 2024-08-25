using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class CodigoBarras
    {
        public CodigoBarras()
        {
                
        }

        public CodigoBarras(string codigo)
        {
            Codigo = codigo;
        }
        public int IdCodigoBarras { get; set; }
        public string Codigo { get; set; }
        public string? Descripcion { get; set; }
        public int IdProducto { get; set; }
        public virtual Product? Product { get; set; }
    }
}
