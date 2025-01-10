using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
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
        Task<VentaWeb> Update(Ajustes? ajustes, VentaWeb entity);
        Task<VentaWeb> Get(int idVentaWeb);
        Task<VentaWeb> RegisterWeb(VentaWeb entity);
        Task<List<VentaWeb>> GetAllByDate(DateTime? registrationDate);

    }
}
