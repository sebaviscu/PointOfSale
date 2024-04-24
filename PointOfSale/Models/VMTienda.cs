using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMTienda
    {
        public int IdTienda { get; set; }
        public string? Nombre { get; set; }
        public ListaDePrecio? IdListaPrecio { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? NombreImpresora { get; set; }
        public byte[]? Logo { get; set; }
        public string? PhotoBase64 { get; set; }
        public bool? Principal { get; set; }
    }
}
