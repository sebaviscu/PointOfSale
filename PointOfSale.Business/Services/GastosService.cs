using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class GastosService : IGastosService
    {
        private readonly IGenericRepository<Gastos> _repository;
        private readonly IGenericRepository<TipoDeGasto> _repositoryTipoDeGasto;

        public GastosService(IGenericRepository<Gastos> repository, IGenericRepository<TipoDeGasto> repositoryTipoDeGasto)
        {
            _repository = repository;
            _repositoryTipoDeGasto = repositoryTipoDeGasto;
        }

        public async Task<List<Gastos>> List()
        {

            IQueryable<Gastos> query = await _repository.Query();
            var resp =  query.Include(_=>_.TipoDeGasto).Include(_ => _.User).OrderBy(_ => _.TipoDeGasto.Descripcion).ToList();
            return resp;
        }

        public async Task<Gastos> Add(Gastos entity)
        {
            try
            {
                Gastos Gastos_created = await _repository.Add(entity);
                if (Gastos_created.IdGastos == 0)
                    throw new TaskCanceledException("Gastos no se pudo crear.");

                return Gastos_created;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Gastos> Edit(Gastos entity)
        {
            try
            {
                Gastos Gastos_found = await _repository.Get(c => c.IdGastos == entity.IdGastos);

                Gastos_found.TipoDeGasto = entity.TipoDeGasto;
                Gastos_found.Comentario = entity.Comentario;
                Gastos_found.Importe = entity.Importe;
                Gastos_found.IdUsuario = entity.IdUsuario;
                Gastos_found.ModificationDate = DateTime.Now;
                Gastos_found.ModificationUser = entity.ModificationUser;

                bool response = await _repository.Edit(Gastos_found);

                if (!response)
                    throw new TaskCanceledException("Gastos no se pudo cambiar.");

                return Gastos_found;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Delete(int idGastos)
        {
            try
            {
                Gastos Gastos_found = await _repository.Get(c => c.IdGastos == idGastos);

                if (Gastos_found == null)
                    throw new TaskCanceledException("The Gastos no existe");


                bool response = await _repository.Delete(Gastos_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<TipoDeGasto>> ListTipoDeGasto()
        {
            IQueryable<TipoDeGasto> query = await _repositoryTipoDeGasto.Query();
            var resp = query.OrderBy(_ => _.Descripcion).ToList();
            return resp;
        }

        public async Task<TipoDeGasto> AddTipoDeGasto(TipoDeGasto entity)
        {
            try
            {
                TipoDeGasto Gastos_created = await _repositoryTipoDeGasto.Add(entity);
                if (Gastos_created.IdTipoGastos == 0)
                    throw new TaskCanceledException("Gastos no se pudo crear.");

                return Gastos_created;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> DeleteTipoDeGasto(int IdTipoGastos)
        {
            try
            {
                TipoDeGasto Gastos_found = await _repositoryTipoDeGasto.Get(c => c.IdTipoGastos == IdTipoGastos);

                if (Gastos_found == null)
                    throw new TaskCanceledException("The Gastos no existe");


                bool response = await _repositoryTipoDeGasto.Delete(Gastos_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
