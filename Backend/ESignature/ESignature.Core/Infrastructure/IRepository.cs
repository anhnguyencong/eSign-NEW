namespace ESignature.Core.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Query;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IRepository<TEntity> where TEntity : class
    {
        void ChangeTable(string table);

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        IQueryable<TEntity> FromSql(string sql, params object[] parameters);

        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null);

        Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate = null);

        Task<T> MaxAsync<T>(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, T>> selector = null);

        Task<T> MinAsync<T>(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, T>> selector = null);

        Task<decimal> AverageAsync(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, decimal>> selector = null);

        Task<decimal> SumAsync(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, decimal>> selector = null);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> selector = null);

        Task AddAsync(TEntity entities);

        Task AddRangeAsync(IList<TEntity> entities);

        void Update(TEntity entity);

        void UpdateRange(IList<TEntity> entities);

        Task<int> UpdateBatchAsync(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TEntity>> updateFactory);

        void Remove(TEntity entity);

        void RemoveRange(IList<TEntity> entities);

        void ChangeEntityState(TEntity entity, EntityState state);
    }
}