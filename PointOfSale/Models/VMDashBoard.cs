namespace PointOfSale.Models
{
    public class VMDashBoard
    {
        public string TotalSales { get; set; }
        public string TotalSalesComparacion { get; set; }
        public List<decimal> SalesList { get; set; }
        public List<decimal> SalesListComparacion { get; set; }
        public List<VMVentasPorTipoDeVenta> VentasPorTipoVenta { get; set; }

        public string[] EjeX { get; set; }
        public string Actual { get; set; }
        public string Anterior { get; set; }
        public string EjeXLeyenda { get; set; }

    }
}
