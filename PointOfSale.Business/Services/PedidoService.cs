using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class PedidoService : IPedidoService
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

        private readonly IGenericRepository<Pedido> _repository;
        private readonly IGenericRepository<PedidoProducto> _repositoryPedidoProducto;
        private readonly IGenericRepository<ProveedorMovimiento> _proveedorMovimiento;
        private readonly IProductService _pruductosRepository;
        private readonly POINTOFSALEContext _dbcontext;

        public PedidoService(IGenericRepository<Pedido> repository,
            IGenericRepository<PedidoProducto> repositoryPedidoProducto,
            IGenericRepository<ProveedorMovimiento> proveedorMovimiento,
            IProductService pruductosRepository,
            POINTOFSALEContext dbcontext)
        {
            _repository = repository;
            _repositoryPedidoProducto = repositoryPedidoProducto;
            _proveedorMovimiento = proveedorMovimiento;
            _pruductosRepository = pruductosRepository;
            _dbcontext = dbcontext;
        }

        public async Task<List<Pedido>> List(int idTienda)
        {
            try
            {
                IQueryable<Pedido> query = await _repository.Query(_ => _.IdTienda == idTienda);
                var resp = query.Include(_ => _.Proveedor).Include(_ => _.Productos).ToList();
                return resp;
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<Pedido> Add(Pedido entity)
        {
            try
            {
                entity.RegistrationDate = DateTimeNowArg;

                Pedido Pedido_created = await _repository.Add(entity);
                if (Pedido_created.IdPedido == 0)
                    throw new TaskCanceledException("Pedido no se pudo crear.");

                IQueryable<Pedido> query = await _repository.Query(_ => _.IdPedido == Pedido_created.IdPedido);
                return query.Include(_ => _.Proveedor).Include(_ => _.Productos).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }

        public async Task<Pedido> Recibir(Pedido entity)
        {
            try
            {
                // movimiento proveedor

                Pedido Pedido_found = await _repository.Get(c => c.IdPedido == entity.IdPedido && c.Estado == Model.Enum.EstadoPedido.Iniciado);
                Pedido_found.Estado = Model.Enum.EstadoPedido.Recibido;

                bool response = await _repository.Edit(Pedido_found);

                if (!response)
                    throw new TaskCanceledException("Pedido no se pudo cambiar.");

                IQueryable<Pedido> query = await _repository.Query(_ => _.IdPedido == Pedido_found.IdPedido);
                return query.Include(_ => _.Proveedor).Include(_ => _.Productos).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }

        public async Task<Pedido> Edit(Pedido entity)
        {
            try
            {
                IQueryable<Pedido> query = await _repository.Query(_ => _.IdPedido == entity.IdPedido/* && _.Estado == Model.Enum.EstadoPedido.Enviado*/);
                var Pedido_found = query.Include(_ => _.Proveedor).Include(_ => _.Productos).FirstOrDefault();

                if (Pedido_found == null)
                    throw new TaskCanceledException("El Pedido no se puede cerrar");

                Pedido_found.ImporteEstimado = entity.ImporteEstimado;
                Pedido_found.Estado = entity.Estado;
                Pedido_found.Comentario = entity.Comentario;
                Pedido_found.Productos = entity.Productos;

                bool response = await _repository.Edit(Pedido_found);

                if (!response)
                    throw new TaskCanceledException("Pedido no se pudo cambiar.");


                return Pedido_found;
            }
            catch
            {
                throw;
            }
        }
        public async Task<Pedido> CerrarPedido(Pedido entity)
        {
            using (var transaction = _dbcontext.Database.BeginTransaction())
            {
                try
                {
                    IQueryable<Pedido> query = await _repository.Query(_ => _.IdPedido == entity.IdPedido && _.Estado != Model.Enum.EstadoPedido.Recibido);
                    var Pedido_found = query.Include(_ => _.Proveedor).Include(_ => _.Productos).FirstOrDefault();

                    if (Pedido_found == null)
                        throw new TaskCanceledException("El Pedido no se puede editar");

                    Pedido_found.ImporteFinal = entity.ImporteEstimado;
                    Pedido_found.FechaCerrado = DateTimeNowArg;
                    Pedido_found.Estado = Model.Enum.EstadoPedido.Recibido;
                    Pedido_found.Comentario = entity.Comentario;
                    Pedido_found.Productos = entity.Productos;
                    Pedido_found.UsuarioFechaCerrado = entity.UsuarioFechaCerrado;

                    bool response = await _repository.Edit(Pedido_found);

                    if (!response)
                        throw new TaskCanceledException("Pedido no se pudo cambiar.");

                    await _proveedorMovimiento.Add(entity.ProveedorMovimiento);

                    await _pruductosRepository.ActualizarStockAndVencimientos(entity.Productos.ToList(), Pedido_found.IdTienda, entity.UsuarioFechaCerrado);

                    transaction.Commit();

                    return Pedido_found;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }

        }

        public async Task<bool> Delete(int idPedido)
        {
            try
            {
                var PedidoProducto_found = await _repositoryPedidoProducto.Query(c => c.IdPedido == idPedido);
                var PedidoProducto_foundList = PedidoProducto_found.ToList();

                if (PedidoProducto_foundList.Any())
                {
                    _ = await _repositoryPedidoProducto.Delete(PedidoProducto_foundList);
                }

                Pedido Pedido_found = await _repository.Get(c => c.IdPedido == idPedido && c.Estado == Model.Enum.EstadoPedido.Iniciado);

                if (Pedido_found == null)
                    throw new TaskCanceledException("El Pedido no existe o no se puede eliminar");

                bool response = await _repository.Delete(Pedido_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
