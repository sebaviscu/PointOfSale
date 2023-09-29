using PointOfSale.Business.Services;
using PointOfSale.Model;
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
        Task<Dictionary<string, string?>> ProductsTopByCategory(TypeValuesDashboard typeValues, string category, int idTienda);
        Task<Dictionary<string, decimal>> GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard typeValues, int turno, int idTienda);
        Task<Dictionary<string, decimal>> GetMovimientosProveedoresByTienda(TypeValuesDashboard typeValues, int idTienda);
        Task<Dictionary<string, decimal>> GetGastos(TypeValuesDashboard typeValues, int idTienda);
        Task<Dictionary<string, decimal>> GetSalesByTypoVentaByTurno(TypeValuesDashboard typeValues, int turno, int idTienda);
    }
}
