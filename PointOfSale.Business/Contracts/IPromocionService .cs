using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IPromocionService
    {
        Task<List<Promocion>> List(int idTienda);
        Task<List<Promocion>> Activas(int idTienda);
        Task<Promocion> Add(Promocion entity);
        Task<Promocion> Edit(Promocion entity);
        Task<bool> Delete(int idUser);
    }
}
