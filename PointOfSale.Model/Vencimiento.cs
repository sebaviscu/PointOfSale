using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class Vencimiento
    {
        public int IdVencimiento { get; set; }
        public string? Lote { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaElaboracion { get; set; }
        public bool Notificar {  get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }

        public Product? Producto { get; set; }
        public int IdProducto { get; set; }

        public Tienda? Tienda { get; set; }
        public int IdTienda { get; set; }
    }
}
