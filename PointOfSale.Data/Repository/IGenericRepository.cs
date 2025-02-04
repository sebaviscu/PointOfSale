﻿using Microsoft.EntityFrameworkCore.Storage;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Data.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetAsNoTracking(Expression<Func<TEntity, bool>> filter);
        Task<TEntity> Get(Expression<Func<TEntity, bool>> filter);
        Task<TEntity> Add(TEntity entity);
        Task<List<TEntity>> AddRange(List<TEntity> entity);
        Task<bool> Edit(TEntity entity);
        Task<bool> EditRange(List<TEntity> entity);
        Task<bool> Delete(TEntity entity);
        Task<IQueryable<TEntity>> Query(Expression<Func<TEntity, bool>> filter = null);
        IQueryable<TEntity> SqlRaw(string query);
        Task<TEntity?> First(Expression<Func<TEntity, bool>> filter = null);
        Task<bool> Delete(List<TEntity> entity);

        Task<TEntity> AddAsync(TEntity entity);
        Task<bool> EditAsync(TEntity entity);
        Task<bool> SaveChangesAsync();
        IQueryable<TEntity> QuerySimple();

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        IExecutionStrategy CreateExecutionStrategy();

    }
}
