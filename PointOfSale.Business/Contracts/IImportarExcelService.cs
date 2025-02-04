using Microsoft.AspNetCore.Http;
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
        Task<(bool exito, List<Product>? productos, List<string> errores)> ImportarProductoAsync(IFormFile file, bool modificarPrecio, bool productoWeb, bool createSku);
    }
}
