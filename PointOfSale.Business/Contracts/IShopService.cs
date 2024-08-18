using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IShopService
    {
        Task<List<VentaWeb>> List();

        Task<VentaWeb> Update(VentaWeb entity);
        Task<VentaWeb> Get(int idVentaWeb);
        Task<VentaWeb> RegisterWeb(VentaWeb entity);

        Task FacturarVentaWeb(VentaWeb VentaWeb_found, string cuil, int? idCliente);

    }
}
