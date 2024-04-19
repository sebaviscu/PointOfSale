using PointOfSale.Model;
using PointOfSale.Model.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IIvaService
    {
        List<DatesFilterIvaReportOutput> GetDatesFilterList(int idTienda);

        Task<List<Gastos>> GetGastosImports(int idTienda, DateTime start_date, DateTime end_date);

        Task<List<Sale>> GetSaleImports(int idTienda, DateTime start_date, DateTime end_date);

        Task<List<ProveedorMovimiento>> GetMovProveedoresImports(int idTienda, DateTime start_date, DateTime end_date);
    }
}
