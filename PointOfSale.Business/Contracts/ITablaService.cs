using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface ITablaService
    {
        Task<List<FormatosVenta>> ListFormatosVenta();

        Task<List<FormatosVenta>> ListFormatosVentaActive();
    }
}
