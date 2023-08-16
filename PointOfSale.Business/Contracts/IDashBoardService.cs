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
        Task<GraficoVentasConComparacion> GetSales(TypeValuesDashboard typeValues, int idTienda);
        Task<Dictionary<string, decimal>> GetSalesByTypoVenta(TypeValuesDashboard typeValues, int idTienda);
        Task<Dictionary<string, decimal?>> ProductsTopByCategory(TypeValuesDashboard typeValues, string category, int idTienda);
        Task<Dictionary<string, decimal>> GetSalesByTypoVentaByTurno(TypeValuesDashboard typeValues, int turno, int idTienda);
    }
}
