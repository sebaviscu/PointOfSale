using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model.Auditoria
{
    public class AVentaWeb
    {
        public AVentaWeb(VentaWeb ventaWeb)
        {
            Estado = ventaWeb.Estado;
        }
        public EstadoVentaWeb Estado { get; set; }
    }
}
