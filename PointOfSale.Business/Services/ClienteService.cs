﻿using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
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

        private readonly IGenericRepository<Cliente> _repository;
        private readonly IGenericRepository<ClienteMovimiento> _clienteMovimiento;
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;
        public ClienteService(IGenericRepository<Cliente> repository, IGenericRepository<ClienteMovimiento> clienteMovimiento, ITypeDocumentSaleService typeDocumentSaleService)
        {
            _repository = repository;
            _clienteMovimiento = clienteMovimiento;
            _typeDocumentSaleService = typeDocumentSaleService;
        }

        public async Task<List<Cliente>> List(int idTienda)
        {
            IQueryable<Cliente> query = await _repository.Query(_ => _.IdTienda == idTienda);
            return query.Include(_ => _.ClienteMovimientos).OrderBy(_ => _.Nombre).ToList();
        }

        public async Task<Cliente> Add(Cliente entity)
        {
            Cliente Cliente_exists = await _repository.Get(u => u.Nombre == entity.Nombre && u.IdTienda == entity.IdTienda);

            if (Cliente_exists != null)
                throw new TaskCanceledException("El Cliente ya existe");

            entity.RegistrationDate = TimeHelper.GetArgentinaTime();
            Cliente Cliente_created = await _repository.Add(entity);

            if (Cliente_created.IdCliente == 0)
                throw new TaskCanceledException("Error al crear Cliente");

            IQueryable<Cliente> query = await _repository.Query(u => u.IdCliente == Cliente_created.IdCliente);

            return Cliente_created;

        }

        public async Task<Cliente> Edit(Cliente entity)
        {
            IQueryable<Cliente> queryCliente = await _repository.Query(u => u.IdCliente == entity.IdCliente);

            Cliente Cliente_edit = queryCliente.First();

            Cliente_edit.Nombre = entity.Nombre;
            Cliente_edit.Cuil = entity.Cuil;
            Cliente_edit.Telefono = entity.Telefono;
            Cliente_edit.Direccion = entity.Direccion;
            Cliente_edit.Comentario = entity.Comentario;
            Cliente_edit.CondicionIva = entity.CondicionIva;
            Cliente_edit.IsActive = entity.IsActive;
            Cliente_edit.ModificationDate = TimeHelper.GetArgentinaTime();
            Cliente_edit.ModificationUser = entity.ModificationUser;

            bool response = await _repository.Edit(Cliente_edit);
            if (!response)
                throw new TaskCanceledException("No se pudo modificar Cliente");

            return Cliente_edit;
        }

        public async Task<bool> Delete(int idCliente)
        {
            Cliente Cliente_found = await _repository.Get(u => u.IdCliente == idCliente);

            if (Cliente_found == null)
                throw new TaskCanceledException("Cliente no existe");

            bool response = await _repository.Delete(Cliente_found);

            return response;

        }

        public async Task<ClienteMovimiento> RegistrarMovimiento(int idCliente, decimal total, string registrationUser, int idTienda, int? idSale, TipoMovimientoCliente tipo)
        {
            var mc = new ClienteMovimiento(idCliente, total, registrationUser, idTienda, idSale)
            {
                TipoMovimiento = tipo
            };
            return await _clienteMovimiento.Add(mc);
        }


        public async Task<List<ClienteMovimiento>> ListMovimientoscliente(int idCliente, int idTienda)
        {
            IQueryable<ClienteMovimiento> query = await _clienteMovimiento.Query(u => u.IdCliente == idCliente && u.IdTienda == idTienda);
            var result = query.Include(_ => _.Sale).OrderByDescending(_ => _.RegistrationUser).ToList();
            result.Where(_ => _.TipoMovimiento == TipoMovimientoCliente.Gastos).ToList().ForEach(_ =>
            {
                _.Total = _.Total * -1;
            }
            );
            return query.ToList();
        }
        public async Task<List<ClienteMovimiento>> GetClienteByMovimientos(List<int>? idMovs, int idTienda)
        {
            IQueryable<ClienteMovimiento> query = await _clienteMovimiento.Query(u => idMovs.Contains(u.IdClienteMovimiento) && u.IdTienda == idTienda);
            var result = query.Include(_ => _.Cliente).ToList();
            return result;
        }

        public async Task RegistrarionClient(Sale sale, decimal importe, string registrationUser, int IdTienda, int idsale, TipoMovimientoCliente? tipoMovimientoCliente, int? ClientId)
        {
            if (ClientId.HasValue)
            {
                var mov = await RegistrarMovimiento(ClientId.Value, importe, registrationUser, IdTienda, idsale, tipoMovimientoCliente.Value);

                if(tipoMovimientoCliente == TipoMovimientoCliente.Pagos)
                {
                    var tipoVenta = await _typeDocumentSaleService.Get(sale.IdTypeDocumentSale.Value);
                    sale.TipoFactura = tipoVenta.TipoFactura;
                }

                var client = await _repository.Get(_ => _.IdCliente == ClientId.Value);
                sale.IdClienteMovimiento = mov.IdClienteMovimiento;
                sale.ClientName = client.Nombre;
            }
        }
    }
}
