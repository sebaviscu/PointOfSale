using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class Ajustes
    {
        public int IdAjuste { get; set; }

        public string? CodigoSeguridad { get; set; }
        public bool? ImprimirDefault { get; set; }
        public bool? ControlStock { get; set; }
        public bool? FacturaElectronica { get; set; }
        public bool? ControlEmpleado { get; set; }
        public string? Encabezado1 { get; set; }
        public string? Encabezado2 { get; set; }
        public string? Encabezado3 { get; set; }
        public string? Pie1 { get; set; }
        public string? Pie2 { get; set; }
        public string? Pie3 { get; set; }
        public string? NombreImpresora { get; set; }
        public long? MinimoIdentificarConsumidor { get; set; }

        public bool? NotificarEmailCierreTurno { get; set; }
        public bool? ControlTotalesCierreTurno { get; set; }

        public string? EmailEmisorCierreTurno {  get; set; }
        public string? PasswordEmailEmisorCierreTurno {  get; set; }
        public string? EmailsReceptoresCierreTurno { get; set; }

        public int IdTienda { get; set; }
        public Tienda? Tienda { get; set; }

        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
