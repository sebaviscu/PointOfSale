using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IImportarExcelService
    {
        Task<(bool, List<Product>?)> ImportarProductoAsync(string filePath);
    }
}
