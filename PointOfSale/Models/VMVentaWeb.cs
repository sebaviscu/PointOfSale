using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMVentaWeb
    {

        public int IdVentaWeb { get; set; }
        public string? SaleNumber { get; set; }
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Comentario { get; set; }
        public int? IdFormaDePago { get; set; }
        public int? IdUsers { get; set; }
        public decimal? Total { get; set; }
        public string? TotalString { get; set; }
        public EstadoVentaWeb? Estado { get; set; }
        public int? IdTienda { get; set; }
        public virtual ICollection<VMDetailSale>? DetailSales { get; set; }
        public string? Fecha { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? FormaDePago { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }

        public bool? IsEdit { get; set; }
        public string? EditText { get; set; }

        public int? IdClienteFactura { get; set; }
        public string? CuilFactura { get; set; }

        public string? NombreImpresora { get; set; }
        public string? Ticket { get; set; } = string.Empty;
        public List<Images> ImagesTicket { get; set; } = new List<Images>();

        public bool ImprimirTicket { get; set; }
        public decimal? DescuentoRetiroLocal { get; set; }
        public string? CruceCallesDireccion { get; set; }
        public decimal? CostoEnvio { get; set; }
        public string? ObservacionesUsuario { get; set; }
        public TipoFactura? TipoFactura { get; set; }


        public decimal? TotalConEnvio => Total + (CostoEnvio != null ? CostoEnvio : 0);
    }
}
