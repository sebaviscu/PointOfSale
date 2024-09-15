using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;

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

            var entityQuery = await _repository.Query();
            var entityFound = entityQuery.AsEnumerable().FirstOrDefault(e => (int)e.GetType().GetProperty("Id" + typeof(T).Name).GetValue(e) == id);

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

            var entityQuery = await _repository.Query();
            var entityFound = entityQuery.AsEnumerable().FirstOrDefault(e => (int)e.GetType().GetProperty("Id" + typeof(T).Name).GetValue(e) == idValue);

            if (entityFound == null)
                throw new TaskCanceledException(typeof(T).Name + " no existe.");

            foreach (var property in entity.GetType().GetProperties())
            {
                if (property.CanWrite)
                {
                    property.SetValue(entityFound, property.GetValue(entity));
                }
            }

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

            var query = (await _repository.Query()).AsEnumerable()
                .Where(e => (bool)estadoProperty.GetValue(e));

            return query.ToList();
        }


    }

}
