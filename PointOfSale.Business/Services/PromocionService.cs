using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class PromocionService : IPromocionService
    {
        private readonly IGenericRepository<Promocion> _repository;

        public PromocionService(IGenericRepository<Promocion> repository)
        {
            _repository = repository;
        }

        public async Task<List<Promocion>> List(int idTienda)
        {
            IQueryable<Promocion> query = await _repository.Query(_ => _.IdTienda == idTienda);
            return query.OrderByDescending(_ => _.IdPromocion).ToList();
        }

        public async Task<List<Promocion>> Activas(int idTienda)
        {
            IQueryable<Promocion> query = await _repository.Query(_ => _.IsActive && _.IdTienda == idTienda);
            return query.OrderByDescending(_ => _.IdPromocion).ToList();
        }

        public async Task<Promocion> Add(Promocion entity)
        {
            try
            {
                entity.RegistrationDate = TimeHelper.GetArgentinaTime();
                Promocion Promocion_created = await _repository.Add(entity);

                if (Promocion_created.IdPromocion == 0)
                    throw new TaskCanceledException("Error al crear Promocion");

                return Promocion_created;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Promocion> Edit(Promocion entity)
        {
            try
            {
                IQueryable<Promocion> queryPromocion = await _repository.Query(u => u.IdPromocion == entity.IdPromocion);

                Promocion Promocion_edit = queryPromocion.First();

                Promocion_edit.Nombre = entity.Nombre;
                Promocion_edit.Porcentaje = entity.Porcentaje;
                Promocion_edit.CantidadProducto = entity.CantidadProducto;
                Promocion_edit.IdProducto = entity.IdProducto;
                Promocion_edit.Dias = entity.Dias;
                Promocion_edit.IdCategory = entity.IdCategory;
                Promocion_edit.IsActive = entity.IsActive;
                Promocion_edit.ModificationDate = TimeHelper.GetArgentinaTime();
                Promocion_edit.ModificationUser = entity.ModificationUser;
                Promocion_edit.Operador = entity.Operador;
                Promocion_edit.Precio = entity.Precio;


                bool response = await _repository.Edit(Promocion_edit);
                if (!response)
                    throw new TaskCanceledException("No se pudo modificar Promocion");

                return Promocion_edit;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> Delete(int idPromocion)
        {
            try
            {
                Promocion Promocion_found = await _repository.Get(u => u.IdPromocion == idPromocion);

                if (Promocion_found == null)
                    throw new TaskCanceledException("Promocion no existe");

                bool response = await _repository.Delete(Promocion_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<Promocion> CambiarEstado(int idPromocion, string userName)
        {
            try
            {
                IQueryable<Promocion> queryPromocion = await _repository.Query(u => u.IdPromocion == idPromocion);

                Promocion Promocion_edit = queryPromocion.First();

                Promocion_edit.IsActive = !Promocion_edit.IsActive;
                Promocion_edit.ModificationDate = TimeHelper.GetArgentinaTime();
                Promocion_edit.ModificationUser = userName;


                bool response = await _repository.Edit(Promocion_edit);
                if (!response)
                    throw new TaskCanceledException("No se pudo modificar Promocion");

                return Promocion_edit;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
