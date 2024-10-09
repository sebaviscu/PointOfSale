using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PointOfSale.Data.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Data.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly POINTOFSALEContext _dbcontext;
        private IDbContextTransaction _transaction;

        public GenericRepository(POINTOFSALEContext context)
        {
            try
            {

            _dbcontext = context;
            }
            catch (Exception e)
            {

                throw;
            }
        }


        public async Task<TEntity> Get(Expression<Func<TEntity, bool>> filter)
        {
            try
            {
                TEntity entity = await _dbcontext.Set<TEntity>().FirstOrDefaultAsync(filter);
                return entity;
            }
            catch
            {
                throw;
            }
        }
        public async Task<TEntity> GetAsNoTracking(Expression<Func<TEntity, bool>> filter)
        {
            try
            {
                IQueryable<TEntity> queryentity = filter == null ? _dbcontext.Set<TEntity>().AsNoTracking() : _dbcontext.Set<TEntity>().AsNoTracking().Where(filter);
                return queryentity.FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }

        public async Task<TEntity> Add(TEntity entity)
        {
            try
            {

                _dbcontext.Set<TEntity>().Add(entity);
                await _dbcontext.SaveChangesAsync();
                return entity;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<TEntity>> AddRange(List<TEntity> entity)
        {
            try
            {

                _dbcontext.Set<TEntity>().AddRange(entity);
                await _dbcontext.SaveChangesAsync();
                return entity;
            }
            catch
            {
                throw;
            }
        }
        public async Task<TEntity> AddAsync(TEntity entity)
        {
            try
            {
                _dbcontext.Set<TEntity>().AddAsync(entity);
                return entity;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> EditAsync(TEntity entity)
        {
            try
            {
                _dbcontext.Update(entity);
                return true;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> EditRange(List<TEntity> entity)
        {
            try
            {
                _dbcontext.UpdateRange(entity);
                return true;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Edit(TEntity entity)
        {
            try
            {
                _dbcontext.Update(entity);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Delete(TEntity entity)
        {
            try
            {
                _dbcontext.Remove(entity);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> Delete(List<TEntity> entity)
        {
            try
            {
                _dbcontext.RemoveRange(entity);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IQueryable<TEntity>> Query(Expression<Func<TEntity, bool>> filter)
        {
            IQueryable<TEntity> queryentity = filter == null ? _dbcontext.Set<TEntity>() : _dbcontext.Set<TEntity>().Where(filter);
            return queryentity;
        }

        public IQueryable<TEntity> SqlRaw(string query)
        {
            return _dbcontext.Set<TEntity>().FromSqlRaw(query);
        }

        public async Task<TEntity?> First(Expression<Func<TEntity, bool>> filter)
        {
            IQueryable<TEntity> queryentity = filter == null ? _dbcontext.Set<TEntity>() : _dbcontext.Set<TEntity>().Where(filter);
            return queryentity.FirstOrDefault();
        }

        public IQueryable<TEntity> QuerySimple()
        {
            return _dbcontext.Set<TEntity>();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _dbcontext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction.RollbackAsync();
        }
    }
}
