using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IGastosService
    {
        Task<List<Gastos>> List(int idTienda);
        Task<Gastos> Add(Gastos entity);
        Task<Gastos> Edit(Gastos entity);
        Task<bool> Delete(int idGastos);
        Task<TipoDeGasto> AddTipoDeGasto(TipoDeGasto entity);
        Task<List<TipoDeGasto>> ListTipoDeGasto();
        Task<bool> DeleteTipoDeGasto(int IdTipoGastos);
        Task<List<Gastos>> ListGastosForTablaDinamica(int idTienda);
        Task<TipoDeGasto> EditTipoGastos(TipoDeGasto entity);
    }
}
