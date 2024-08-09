using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class AjustesFacturacion
    {
        public int IdAjustesFacturacion { get; set; }

        public long? Cuit { get; set; }
        public int? PuntoVenta { get; set; }
        public CondicionIva? CondicionIva { get; set; }

        public string? CertificadoPassword { get; set; }
        public string? CertificadoNombre { get; set; }
        public DateTime? CertificadoFechaInicio { get; set; }
        public DateTime? CertificadoFechaCaducidad { get; set; }

        public int IdTienda { get; set; }
        public Tienda? Tienda { get; set; }

        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
