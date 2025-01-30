using PointOfSale.Business.Utilities;using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public partial class Turno
    {
        public Turno()
        {
            ValidacionRealizada = false;
        }

        public Turno(int idTienda, string usuario)
        {
            FechaInicio = TimeHelper.GetArgentinaTime();
            IdTienda = idTienda;
            RegistrationUser = usuario;
            RegistrationDate= TimeHelper.GetArgentinaTime();
        }
        public int IdTurno { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int IdTienda { get; set; }
        public Tienda? Tienda { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string RegistrationUser { get; set; }
        public string? ModificationUser { get; set; }

		public virtual ICollection<Sale> Sales { get; set; }

        public string? ObservacionesApertura { get; set; }
        public string? ObservacionesCierre { get; set; }
        public decimal TotalInicioCaja { get; set; }

        public decimal? TotalCierreCajaSistema { get; set; }
        public decimal? TotalCierreCajaReal { get; set; }
        public string? ErroresCierreCaja { get; set; }
        public bool? ValidacionRealizada { get; set; }


        public string? BilletesEfectivo { get; set; }
        public ICollection<VentasPorTipoDeVentaTurno>? VentasPorTipoDeVenta { get; set; }
        public ICollection<MovimientoCaja>? MovimientosCaja { get; set; }

    }
}
