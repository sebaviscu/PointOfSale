using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IClienteService
    {
        Task<List<Cliente>> List();
        Task<Cliente> Add(Cliente entity);
        Task<Cliente> Edit(Cliente entity);
        Task<bool> Delete(int idUser);

        Task<ClienteMovimiento> RegistrarMovimiento(int idCliente, decimal total, string registrationUser, int? idSale);
    }
}
