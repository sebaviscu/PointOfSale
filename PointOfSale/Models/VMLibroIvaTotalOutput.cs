using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMLibroIvaTotalOutput
    {
        public List<VMIvaRowOutput> IvaRows { get; set; }
        public decimal TotalFacurado { get; set; }
        public decimal? TotalIva { get; set; }

        public decimal? TotalSinIva { get; set; }
        public string? Nombre { get; set; }
    }
}
