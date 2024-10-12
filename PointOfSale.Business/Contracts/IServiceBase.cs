using System.Linq.Expressions;

namespace PointOfSale.Business.Contracts
{
    public interface IServiceBase<T> where T : class
    {
        Task<T> Add(T entity);

        Task<bool> Delete(int id);

        Task<T> Edit(T entity);

        IQueryable<T> List();

        IQueryable<T> ListActive();

        Task<IQueryable<T>> IncludeDetails(bool incluideDetails, params Expression<Func<T, object>>[] propertySelectors);

        Task<T?> GetById(int id);
    }
}
