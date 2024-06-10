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
    public class TypeDocumentSaleService : ITypeDocumentSaleService
    {
        private readonly IGenericRepository<TypeDocumentSale> _repository;

        public TypeDocumentSaleService(IGenericRepository<TypeDocumentSale> repository)
        {
            _repository = repository;
        }

        public async Task<TypeDocumentSale> Get(int idTipoVenta)
        {
            return await _repository.First(_ => _.IdTypeDocumentSale == idTipoVenta);
        }

        public async Task<List<TypeDocumentSale>> List()
        {
            IQueryable<TypeDocumentSale> query = await _repository.Query();
            return query.OrderBy(_ => _.Description).ToList();
        }

        public async Task<List<TypeDocumentSale>> GetActive()
        {
            IQueryable<TypeDocumentSale> query = await _repository.Query(u => u.IsActive == true);
            return query.OrderBy(_ => _.Description).ToList();
        }

        public async Task<TypeDocumentSale> Add(TypeDocumentSale entity)
        {
            TypeDocumentSale TypeDocumentSale_exists = await _repository.Get(u => u.Description == entity.Description);

            if (TypeDocumentSale_exists != null)
                throw new TaskCanceledException("The email already exists");

            try
            {

                TypeDocumentSale TypeDocumentSale_created = await _repository.Add(entity);

                if (TypeDocumentSale_created.IdTypeDocumentSale == 0)
                    throw new TaskCanceledException("Error al crear TypeDocumentSale");

                IQueryable<TypeDocumentSale> query = await _repository.Query(u => u.IdTypeDocumentSale == TypeDocumentSale_created.IdTypeDocumentSale);

                return TypeDocumentSale_created;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<TypeDocumentSale> Edit(TypeDocumentSale entity)
        {

            try
            {
                IQueryable<TypeDocumentSale> queryTypeDocumentSale = await _repository.Query(u => u.IdTypeDocumentSale == entity.IdTypeDocumentSale);

                TypeDocumentSale TypeDocumentSale_edit = queryTypeDocumentSale.First();

                TypeDocumentSale_edit.Description = entity.Description;
                TypeDocumentSale_edit.IsActive = entity.IsActive;
                TypeDocumentSale_edit.TipoFactura = entity.TipoFactura;
                TypeDocumentSale_edit.Web = entity.Web;
                TypeDocumentSale_edit.Comision = entity.Comision;

                bool response = await _repository.Edit(TypeDocumentSale_edit);
                if (!response)
                    throw new TaskCanceledException("No se pudo modificar TypeDocumentSale");

                return TypeDocumentSale_edit;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> Delete(int idTypeDocumentSale)
        {
            try
            {
                TypeDocumentSale TypeDocumentSale_found = await _repository.Get(u => u.IdTypeDocumentSale == idTypeDocumentSale);

                if (TypeDocumentSale_found == null)
                    throw new TaskCanceledException("TypeDocumentSaleno existe");

                bool response = await _repository.Delete(TypeDocumentSale_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<TypeDocumentSale>> ListWeb()
        {
            IQueryable<TypeDocumentSale> query = await _repository.Query(_ => _.Web);
            return query.OrderBy(_ => _.Description).ToList();
        }

    }
}
