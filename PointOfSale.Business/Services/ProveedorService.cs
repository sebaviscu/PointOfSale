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
    public class ProveedorService : IProveedorService
    {
        private readonly IGenericRepository<Proveedor> _repository;

        public ProveedorService(IGenericRepository<Proveedor> repository)
        {
            _repository = repository;
        }

        public async Task<List<Proveedor>> List()
        {
            IQueryable<Proveedor> query = await _repository.Query();
            return query.ToList();
        }

        public async Task<Proveedor> Add(Proveedor entity)
        {
            Proveedor Proveedor_exists = await _repository.Get(u => u.Nombre == entity.Nombre);

            if (Proveedor_exists != null)
                throw new TaskCanceledException("El Proveedor ya existe");

            try
            {
                entity.RegistrationDate = DateTime.Now;
                Proveedor Proveedor_created = await _repository.Add(entity);

                if (Proveedor_created.IdProveedor == 0)
                    throw new TaskCanceledException("Error al crear Proveedor");

                IQueryable<Proveedor> query = await _repository.Query(u => u.IdProveedor == Proveedor_created.IdProveedor);

                return Proveedor_created;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Proveedor> Edit(Proveedor entity)
        {

            try
            {
                IQueryable<Proveedor> queryProveedor = await _repository.Query(u => u.IdProveedor == entity.IdProveedor);

                Proveedor Proveedor_edit = queryProveedor.First();

                Proveedor_edit.Nombre = entity.Nombre;
                Proveedor_edit.Cuil = entity.Cuil;
                Proveedor_edit.Telefono = entity.Telefono;
                Proveedor_edit.Direccion = entity.Direccion;
                Proveedor_edit.ModificationDate = DateTime.Now;
                Proveedor_edit.ModificationUser = entity.ModificationUser;

                bool response = await _repository.Edit(Proveedor_edit);
                if (!response)
                    throw new TaskCanceledException("No se pudo modificar Proveedor");

                return Proveedor_edit;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> Delete(int idProveedor)
        {
            try
            {
                Proveedor Proveedor_found = await _repository.Get(u => u.IdProveedor == idProveedor);

                if (Proveedor_found == null)
                    throw new TaskCanceledException("Proveedor no existe");

                bool response = await _repository.Delete(Proveedor_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
