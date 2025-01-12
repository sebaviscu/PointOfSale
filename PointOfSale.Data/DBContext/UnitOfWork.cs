using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointOfSale.Data.Repository;

namespace PointOfSale.Data.DBContext
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly POINTOFSALEContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(POINTOFSALEContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new GenericRepository<T>(_context);
                _repositories[type] = repositoryInstance;
            }

            return (IGenericRepository<T>)_repositories[type];
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}