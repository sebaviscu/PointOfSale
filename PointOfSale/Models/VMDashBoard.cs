namespace PointOfSale.Models
{
    public class VMDashBoard
    {
        public string TotalSales { get; set; }
        public string TotalSalesComparacion { get; set; }
        public List<int> SalesList { get; set; }
        public List<int> SalesListComparacion { get; set; }
        public List<VMVentasPorTipoDeVenta> VentasPorTipoVenta { get; set; }
        public List<VMVentasPorTipoDeVenta> GastosPorTipo { get; set; }
        public List<VMVentasPorTipoDeVenta> GastosPorTipoProveedor { get; set; }

        public int CantidadClientes { get; set; }
        public string GastosTotales { get; set; }
        public string GastosTexto { get; set; }

        public string Ganancia { get; set; }

        public string[] EjeX { get; set; }
        public string Actual { get; set; }
        public string Anterior { get; set; }
        public string EjeXLeyenda { get; set; }

    }
}
