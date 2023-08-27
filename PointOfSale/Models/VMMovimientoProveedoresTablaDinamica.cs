using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMMovimientoProveedoresTablaDinamica
    {
        public decimal Importe { get; set; }
        public decimal? Importe_Sin_Iva { get; set; }
        public decimal? Iva { get; set; }
        public decimal? Iva_Importe { get; set; }
        public string? Nro_Factura { get; set; }
        public string? Tipo_Factura { get; set; }
        public string? Comentario { get; set; }
        public DateTime? Fecha { get; set; }
        public string Nombre_Proveedor { get; set; }
        public string? Cuil { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

    }
}
