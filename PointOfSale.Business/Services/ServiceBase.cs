using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using System.Linq.Expressions;

namespace PointOfSale.Business.Services
{
    public class ServiceBase<T> where T : class
    {
        protected readonly IGenericRepository<T> _repository;

        public ServiceBase(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<T> Add(T entity)
        {
            var entityCreated = await _repository.Add(entity);

            var idProperty = entityCreated.GetType().GetProperty("Id" + typeof(T).Name);

            if (idProperty == null || (int)idProperty.GetValue(entityCreated) == 0)
            {
                throw new TaskCanceledException(typeof(T).Name + " no se pudo crear.");
            }

            return entityCreated;
        }

        public async Task<bool> Delete(int id)
        {
            var entityFound = await _repository.QuerySimple()
                .FirstOrDefaultAsync(e => (int)e.GetType().GetProperty("Id" + typeof(T).Name).GetValue(e) == id);

            if (entityFound == null)
                throw new TaskCanceledException(typeof(T).Name + " no existe.");

            bool response = await _repository.Delete(entityFound);
            return response;
        }


        public async Task<T> Edit(T entity)
        {
            var idProperty = entity.GetType().GetProperty("Id" + typeof(T).Name);

            if (idProperty == null)
                throw new TaskCanceledException("ID no encontrado.");

            var idValue = (int)idProperty.GetValue(entity);

            // Realiza la búsqueda directamente en la base de datos
            var entityFound = await _repository.QuerySimple()
                .FirstOrDefaultAsync(e => (int)e.GetType().GetProperty("Id" + typeof(T).Name).GetValue(e) == idValue);

            if (entityFound == null)
                throw new TaskCanceledException(typeof(T).Name + " no existe.");

            // Actualiza las propiedades de la entidad encontrada
            foreach (var property in entity.GetType().GetProperties())
            {
                if (property.CanWrite)
                {
                    property.SetValue(entityFound, property.GetValue(entity));
                }
            }

            // Realiza la edición en el repositorio
            bool response = await _repository.Edit(entityFound);

            if (!response)
                throw new TaskCanceledException(typeof(T).Name + " no se pudo actualizar.");

            return entityFound;
        }


        public async Task<List<T>> List()
        {
            var query = await _repository.Query();
            return query.ToList();
        }

        public async Task<List<T>> ListActive()
        {
            var estadoProperty = typeof(T).GetProperty("Estado");

            if (estadoProperty == null)
                throw new InvalidOperationException("La entidad no tiene la propiedad 'Estado'.");

            var query = await _repository.Query(e => (bool)EF.Property<bool>(e, estadoProperty.Name));

            return await query.ToListAsync();
        }


        public async Task<IQueryable<T>> IncludeDetails(bool incluideDetails, params Expression<Func<T, object>>[] propertySelectors)
        {
            IQueryable<T> query = await _repository.Query();

            if (incluideDetails && propertySelectors != null && propertySelectors.Length > 0)
            {
                foreach (var propertySelector in propertySelectors)
                {
                    query = query.Include(propertySelector);
                }

            }

            return query;
        }

    }

}
