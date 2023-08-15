using PointOfSale.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Contracts
{
    public interface IDashBoardService
    {
        Task<int> TotalSalesLastWeek();
        Task<string> TotalIncomeLastWeek();
        Task<int> TotalProducts();
        Task<int> TotalCategories();
        Task<GraficoVentasConComparacion> GetSales(TypeValuesDashboard typeValues);
        Task<Dictionary<string, int>> ProductsTop(TypeValuesDashboard typeValues);
        Task<Dictionary<string, decimal>> GetSalesByTypoVenta(TypeValuesDashboard typeValues);
    }
}
