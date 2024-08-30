using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IMovimientoCajaService
    {

        Task<List<RazonMovimientoCaja>> GetRazonMovimientoCaja();
        Task<List<MovimientoCaja>> List(int idTienda);

        Task<RazonMovimientoCaja> Add(RazonMovimientoCaja entity);

        Task<MovimientoCaja> Add(MovimientoCaja entity);

        Task<bool> UpdateEstado(int idEntity);
    }
}
