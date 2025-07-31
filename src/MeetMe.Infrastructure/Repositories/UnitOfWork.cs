using MeetMe.Application.Common.Interfaces;
using MeetMe.Domain.Common;
using MeetMe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MeetMe.Infrastructure.Repositories
{
    /// <summary>
    /// Unit of Work implementation for managing database transactions and repositories
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<string, object> _repositories = new();
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICommandRepository<T> CommandRepository<T>() where T : BaseEntity
        {
            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new CommandRepository<T>(_context);
                _repositories.Add(type, repositoryInstance);
            }

            return (ICommandRepository<T>)_repositories[type];
        }

        public ICommandRepository<T, TKey> CommandRepository<T, TKey>() 
            where T : BaseEntity<TKey> 
            where TKey : IEquatable<TKey>
        {
            var type = $"{typeof(T).Name}_{typeof(TKey).Name}";

            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new CommandRepository<T, TKey>(_context);
                _repositories.Add(type, repositoryInstance);
            }

            return (ICommandRepository<T, TKey>)_repositories[type];
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress.");
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await _transaction.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress.");
            }

            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task<int> SaveChangesAsync(string userId, CancellationToken cancellationToken = default)
        {
            // Update audit fields before saving
            UpdateAuditFields(userId);
            
            return await _context.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields(string userId)
        {
            var entries = _context.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    entry.Entity.CreatedByUserId = userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModifiedDate = DateTime.UtcNow;
                    entry.Entity.LastModifiedByUserId = userId;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            await _context.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
