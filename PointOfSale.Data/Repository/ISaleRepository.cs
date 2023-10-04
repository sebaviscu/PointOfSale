using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Data.Repository
{
    public interface ISaleRepository : IGenericRepository<Sale>
    {
        Task<Sale> Register(Sale entity);
        Task<VentaWeb> RegisterWeb(VentaWeb entity);
        Task<string> GetLastSerialNumberSale();
        Task<List<DetailSale>> Report(DateTime StartDate, DateTime EndDate);
        Task<Sale> CreatSaleFromVentaWeb(VentaWeb entity, Turno turno);

    }
}
