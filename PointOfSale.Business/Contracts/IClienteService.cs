using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Contracts
{
    public interface IClienteService
    {
        Task<List<Cliente>> List(int idTienda);
        Task<Cliente> Add(Cliente entity);
        Task<Cliente> Edit(Cliente entity);
        Task<bool> Delete(int idUser);

        Task<ClienteMovimiento> RegistrarMovimiento(int idCliente, decimal total, string registrationUser, int idTienda, int? idSale, TipoMovimientoCliente tipo);
        Task<List<ClienteMovimiento>> ListMovimientoscliente(int idCliente, int idTienda);
        Task<List<ClienteMovimiento>> GetClienteByMovimientos(List<int>? idMovs, int idTienda);
    }
}
