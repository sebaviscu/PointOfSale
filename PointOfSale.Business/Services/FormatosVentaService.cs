using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class FormatosVentaService : ServiceBase<FormatosVenta>, IFormatosVentaService
    {
        public FormatosVentaService(IGenericRepository<FormatosVenta> repository) : base(repository)
        {
        }

    }
}
