using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMTurno
    {
        public int IdTurno { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? IdTienda { get; set; }
        public Tienda? Tienda { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }
        public string? ModificationUser { get; set; }
        public List<VMVentasPorTipoDeVenta>? VentasPorTipoVenta { get; set; }
        public string? Fecha { get; set; }
        public string? HoraInicio { get; set; }
        public string? HoraFin { get; set; }

        public string? Total { get; set; }

        public Sale? Sale { get; set; }


        public string? ObservacionesApertura { get; set; }
        public string? ObservacionesCierre { get; set; }
        public decimal TotalInicioCaja { get; set; }

        public decimal? TotalCierreCajaSistema { get; set; }
        public decimal? TotalCierreCajaReal { get; set; }
        public string? ErroresCierreCaja { get; set; }
        public bool? ValidacionRealizada { get; set; }

        public int? DiferenciaCierreCaja => (int)((TotalCierreCajaReal != null ? TotalCierreCajaReal.Value : 0) - (TotalCierreCajaSistema != null ? TotalCierreCajaSistema.Value : 0));

        public bool? ImpirmirCierreCaja { get; set; }

    }
}
