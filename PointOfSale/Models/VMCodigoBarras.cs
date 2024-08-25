namespace PointOfSale.Models
{
    public class VMCodigoBarras
    {
        public int IdCodigoBarras { get; set; }
        public string Codigo { get; set; }
        public string? Descripcion { get; set; }
        public int IdProducto { get; set; }
    }
}
