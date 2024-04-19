namespace PointOfSale.Model
{
    public class VMIvaRowOutput
    {
        public DateTime Fecha {  get; set; }
        public string FechaString { get; set; }
        public string TipoFactura { get; set; }
        public string Factura { get; set; }
        public string MetodoPago { get; set; }
        public string? Proveedor { get; set; }
        public decimal Importe { get; set; }
        public decimal? ImporteIva {  get; set; }
        public decimal? ImporteSinIva { get; set; }
        public string? Gastos { get; set; }
        public string? TipoGastos { get; set; }
    }
}
