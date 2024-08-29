namespace PointOfSale.Models
{
    public class VMAjustes
    {
        public int IdAjuste { get; set; }
        public string? CodigoSeguridad { get; set; }
        public bool? ImprimirDefault { get; set; }
        public bool? FacturaElectronica { get; set; }
        public bool? ControlStock { get; set; }
        public bool? ControlEmpleado { get; set; }
        public string? NombreImpresora { get; set; }
        public long? MinimoIdentificarConsumidor { get; set; }
        public string? NombreTiendaTicket { get; set; }
        public int IdTienda { get; set; }

        public bool? NotificarEmailCierreTurno { get; set; }
        public string? EmailEmisorCierreTurno { get; set; }
        public string? PasswordEmailEmisorCierreTurno { get; set; }
        public string? EmailsReceptoresCierreTurno { get; set; }


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
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
