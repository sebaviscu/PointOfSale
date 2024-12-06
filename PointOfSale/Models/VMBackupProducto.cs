using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMBackupProducto
    {
        public int Id { get; set; }
        public string? CorrelativeNumberMasivo { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationUser { get; set; }



        public int IdProduct { get; set; }
        public string? SKU { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public bool IsActive { get; set; }
        public decimal? PriceWeb { get; set; }
        public int? PorcentajeProfit { get; set; }
        public decimal? CostPrice { get; set; }
        public string? Comentario { get; set; }
        public TipoVenta TipoVenta { get; set; }
        public string TipoVentaString => TipoVenta.ToString();
        public decimal? Iva { get; set; }
        public decimal? PrecioFormatoWeb { get; set; }
        public int? FormatoWeb { get; set; }
        public bool Destacado { get; set; }
        public bool ProductoWeb { get; set; }
        public bool? ModificarPrecio { get; set; }
        public bool? PrecioAlMomento { get; set; }
        public bool? ExcluirPromociones { get; set; }

        public string? Category { get; set; }
        public int? IdCategory { get; set; }

        public string? Proveedor { get; set; }
        public int? IdProveedor { get; set; }

        public decimal Precio1 { get; set; }
        public int? PorcentajeProfit1 { get; set; }
        public decimal Precio2 { get; set; }
        public int? PorcentajeProfit2 { get; set; }
        public decimal Precio3 { get; set; }
        public int? PorcentajeProfit3 { get; set; }

    }
}
