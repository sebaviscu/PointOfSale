using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public  class VentasPorTipoDeVentaTurno
    {
        public int Id { get; set; }
        public string? Descripcion { get; set; }
        public decimal? TotalSistema { get; set; }
        public decimal? TotalUsuario { get; set; }
        public string? Error { get; set; }
        public decimal? DiferenciaTotales => TotalUsuario - TotalSistema;

        public int IdTurno { get; set; }
        public Turno? Turno { get; set; }
    }
}
