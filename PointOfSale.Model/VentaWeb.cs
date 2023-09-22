using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class VentaWeb
    {
        public VentaWeb()
        {
            DetailSales = new HashSet<DetailSale>();
        }
        public int IdVentaWeb{ get; set; }
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Comentario { get; set; }
        public int? IdFormaDePago { get; set; }
        public decimal? Total { get; set; }
        public int IdTienda { get; set; }
        public EstadoVentaWeb Estado { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }

        public virtual TypeDocumentSale? FormaDePago { get; set; }
        public virtual ICollection<DetailSale> DetailSales { get; set; }

    }
}
