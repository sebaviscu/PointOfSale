using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMAjustesWeb
    {
        public int idAjusteWeb { get; set; }
        public int IdTienda { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Email { get; set; }

        public decimal? MontoEnvioGratis { get; set; }
        public decimal? CostoEnvio { get; set; }
        public decimal? CompraMinima { get; set; }
        public bool? HabilitarTakeAway { get; set; }
        public decimal? TakeAwayDescuento { get; set; }
        public decimal? AumentoWeb { get; set; }

        public string? Whatsapp { get; set; }
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
        public bool? IvaEnPrecio { get; set; }
        public string? SobreNosotros { get; set; }
        public string? TemrinosCondiciones { get; set; }
        public string? PoliticaPrivacidad { get; set; }
        public string? LogoImagenNombre { get; set; }
        public string? PalabrasClave { get; set; } = string.Empty;
        public string? DescripcionWeb { get; set; } = string.Empty;
        public string? UrlSitio { get; set; } = string.Empty;
        public ICollection<VMHorarioWeb>? HorariosWeb { get; set; }
    }
}
