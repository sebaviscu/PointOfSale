using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IPedidoService
    {
        Task<List<Pedido>> List(int idTienda);

        Task<Pedido> Add(Pedido entity);

        Task<Pedido> Edit(Pedido entity);

        Task<bool> Delete(int idPedido);

        Task<Pedido> Recibir(Pedido entity);

        Task<Pedido> CerrarPedido(Pedido entity);

        Task<List<Pedido>> GetByProveedor(int idTienda, int idProveedor);
    }
}
