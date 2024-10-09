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

            var idProperty = entityCreated.GetType().GetProperty("Id");

            if (idProperty == null || (int)idProperty.GetValue(entityCreated) == 0)
            {
                throw new TaskCanceledException(typeof(T).Name + " no se pudo crear.");
            }

            return entityCreated;
        }

        public async Task<bool> Delete(int id)
        {
            // Crear una expresión para comparar el Id dinámicamente
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, "Id");
            var idValueExpression = Expression.Constant(id);
            var equalExpression = Expression.Equal(property, idValueExpression);

            var lambda = Expression.Lambda<Func<T, bool>>(equalExpression, parameter);

            // Buscar la entidad en la base de datos
            var entityFound = await _repository.QuerySimple()
                .FirstOrDefaultAsync(lambda);  // Usar la expresión generada dinámicamente

            if (entityFound == null)
                throw new TaskCanceledException(typeof(T).Name + " no existe.");

            // Eliminar la entidad encontrada
            bool response = await _repository.Delete(entityFound);
            return response;
        }


        public async Task<T> Edit(T entity)
        {
            // Obtener la propiedad "Id{T}" dinámicamente
            var idProperty = entity.GetType().GetProperty("Id");

            if (idProperty == null)
                throw new TaskCanceledException("ID no encontrado.");

            var idValue = (int)idProperty.GetValue(entity);

            // Crear una expresión para comparar el Id dinámicamente
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, "Id");
            var idValueExpression = Expression.Constant(idValue);
            var equalExpression = Expression.Equal(property, idValueExpression);

            var lambda = Expression.Lambda<Func<T, bool>>(equalExpression, parameter);

            // Buscar la entidad en la base de datos
            var entityFound = await _repository.QuerySimple()
                .FirstOrDefaultAsync(lambda);  // Usar la expresión generada dinámicamente

            if (entityFound == null)
                throw new TaskCanceledException(typeof(T).Name + " no existe.");

            // Actualizar las propiedades de la entidad encontrada
            foreach (var propertyInfo in entity.GetType().GetProperties())
            {
                if(propertyInfo.Name.ToLower() == "registrationdate")
                {
                    continue;
                }

                if (propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(entityFound, propertyInfo.GetValue(entity));
                }
            }

            // Realizar la edición en el repositorio
            bool response = await _repository.Edit(entityFound);

            if (!response)
                throw new TaskCanceledException(typeof(T).Name + " no se pudo actualizar.");

            return entityFound;
        }

        public IQueryable<T> List()
        {
            return _repository.QuerySimple();
        }

        public IQueryable<T> ListActive()
        {
            var estadoProperty = typeof(T).GetProperty("Estado");

            if (estadoProperty == null)
                throw new InvalidOperationException("La entidad no tiene la propiedad 'Estado'.");
            var query = _repository.QuerySimple();
            return query.Where(e => (bool)EF.Property<bool>(e, estadoProperty.Name));
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
