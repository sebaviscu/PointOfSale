using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class AjustesWeb
    {
        public int IdAjusteWeb { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public decimal? MontoEnvioGratis { get; set; }
        public decimal? AumentoWeb { get; set; }
        public string? Whatsapp { get; set; }
        public string? Lunes { get; set; }
        public string? Martes { get; set; }
        public string? Miercoles { get; set; }
        public string? Jueves { get; set; }
        public string? Viernes { get; set; }
        public string? Sabado { get; set; }
        public string? Domingo { get; set; }
        public string? Feriado { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Tiktok { get; set; }
        public string? Twitter { get; set; }
        public string? Youtube { get; set; }
        public bool? HabilitarWeb { get; set; }

        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }

        public string? NombreComodin1 { get; set; }
        public bool? HabilitarComodin1 { get; set; }
        public string? NombreComodin2 { get; set; }
        public bool? HabilitarComodin2 { get; set; }
        public string? NombreComodin3 { get; set; }
        public bool? HabilitarComodin3 { get; set; }
    }
}
