using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
    }
}
