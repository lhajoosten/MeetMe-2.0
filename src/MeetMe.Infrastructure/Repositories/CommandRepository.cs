using MeetMe.Application.Common.Interfaces;
using MeetMe.Domain.Common;
using MeetMe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetMe.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation for write operations (CQRS Command side)
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    public class CommandRepository<T, TKey> : ICommandRepository<T, TKey> 
        where T : BaseEntity<TKey> 
        where TKey : IEquatable<TKey>
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public CommandRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> AddAsync(T entity, int userId, CancellationToken cancellationToken = default)
        {
            entity.CreatedByUserId = userId;
            entity.CreatedDate = DateTime.UtcNow;
            
            var result = await _dbSet.AddAsync(entity, cancellationToken);
            return result.Entity;
        }

        public Task UpdateAsync(T entity, int userId, CancellationToken cancellationToken = default)
        {
            entity.LastModifiedByUserId = userId;
            entity.LastModifiedDate = DateTime.UtcNow;
            
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<bool> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            return true;
        }

        public Task SoftDeleteAsync(T entity, int userId, CancellationToken cancellationToken = default)
        {
            // Assuming entities have an IsActive property for soft delete
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.IsActive = false;
                baseEntity.LastModifiedByUserId = userId;
                baseEntity.LastModifiedDate = DateTime.UtcNow;
            }
            
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Convenience implementation for entities with int keys
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class CommandRepository<T> : CommandRepository<T, int>, ICommandRepository<T> 
        where T : BaseEntity<int>
    {
        public CommandRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
