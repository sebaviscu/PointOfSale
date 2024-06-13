using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class ClienteService : IClienteService
    {
        public DateTime DateTimeNowArg => TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

        private readonly IGenericRepository<Cliente> _repository;
        private readonly IGenericRepository<ClienteMovimiento> _clienteMovimiento;

        public ClienteService(IGenericRepository<Cliente> repository, IGenericRepository<ClienteMovimiento> clienteMovimiento)
        {
            _repository = repository;
            _clienteMovimiento = clienteMovimiento;
        }

        public async Task<List<Cliente>> List(int idTienda)
        {
            IQueryable<Cliente> query = await _repository.Query(_ => _.IdTienda == idTienda);
            return query.Include(_=>_.ClienteMovimientos).OrderBy(_ => _.Nombre).ToList();
        }

        public async Task<Cliente> Add(Cliente entity)
        {
            Cliente Cliente_exists = await _repository.Get(u => u.Nombre == entity.Nombre && u.IdTienda == entity.IdTienda);

            if (Cliente_exists != null)
                throw new TaskCanceledException("El Cliente ya existe");

            try
            {
                entity.RegistrationDate = DateTimeNowArg;
                Cliente Cliente_created = await _repository.Add(entity);

                if (Cliente_created.IdCliente == 0)
                    throw new TaskCanceledException("Error al crear Cliente");

                IQueryable<Cliente> query = await _repository.Query(u => u.IdCliente == Cliente_created.IdCliente);

                return Cliente_created;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Cliente> Edit(Cliente entity)
        {

            try
            {
                IQueryable<Cliente> queryCliente = await _repository.Query(u => u.IdCliente == entity.IdCliente);

                Cliente Cliente_edit = queryCliente.First();

                Cliente_edit.Nombre = entity.Nombre;
                Cliente_edit.Cuit = entity.Cuit;
                Cliente_edit.Telefono = entity.Telefono;
                Cliente_edit.Direccion = entity.Direccion;
                Cliente_edit.ModificationDate = DateTimeNowArg;
                Cliente_edit.ModificationUser = entity.ModificationUser;

                bool response = await _repository.Edit(Cliente_edit);
                if (!response)
                    throw new TaskCanceledException("No se pudo modificar Cliente");

                return Cliente_edit;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> Delete(int idCliente)
        {
            try
            {
                Cliente Cliente_found = await _repository.Get(u => u.IdCliente == idCliente);

                if (Cliente_found == null)
                    throw new TaskCanceledException("Cliente no existe");

                bool response = await _repository.Delete(Cliente_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ClienteMovimiento> RegistrarMovimiento(int idCliente, decimal total, string registrationUser,int idTienda, int? idSale, TipoMovimientoCliente tipo)
        {
            var mc = new ClienteMovimiento(idCliente, total, registrationUser, idTienda, idSale);
            mc.TipoMovimiento = tipo;
            return await _clienteMovimiento.Add(mc);
        }


        public async Task<List<ClienteMovimiento>> ListMovimientoscliente(int idCliente, int idTienda)
        {
            IQueryable<ClienteMovimiento> query = await _clienteMovimiento.Query(u => u.IdCliente == idCliente && u.IdTienda == idTienda);
            var result = query.Include(_ => _.Sale).OrderByDescending(_ => _.RegistrationUser).ToList();
            result.Where(_ => _.TipoMovimiento == TipoMovimientoCliente.Egreso).ToList().ForEach(_ =>
            {
                _.Total = _.Total * -1;
            }
            );
            return query.ToList();
        }
        public async Task<List<ClienteMovimiento>> GetClienteByMovimientos(List<int>? idMovs, int idTienda)
        {
            IQueryable<ClienteMovimiento> query = await _clienteMovimiento.Query(u => idMovs.Contains(u.IdClienteMovimiento) && u.IdTienda == idTienda);
            var result = query.Include(_=>_.Cliente).ToList();
            return result;
        }
    }
}
