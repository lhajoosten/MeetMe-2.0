using MeetMe.Domain.Common;
using System.Linq.Expressions;

namespace MeetMe.Application.Common.Interfaces
{
    /// <summary>
    /// Enhanced generic repository interface for read-only operations with AutoMapper projection support
    /// </summary>
    /// <typeparam name="T">The domain entity type</typeparam>
    /// <typeparam name="TKey">The type of entity's primary key</typeparam>
    public interface IQueryRepository<T, TKey> where T : BaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        #region Entity-based Methods (when you need full entities for business logic)

        /// <summary>
        /// Gets an entity by its ID asynchronously
        /// </summary>
        Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// Gets a queryable collection of entities
        /// </summary>
        IQueryable<T> AsQueryable();

        /// <summary>
        /// Finds entities based on a predicate asynchronously
        /// </summary>
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// Checks if any entity matches the given predicate asynchronously
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entities asynchronously
        /// </summary>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// Counts entities that match a predicate asynchronously
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single entity matching a predicate asynchronously
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// Gets a single entity matching a predicate asynchronously, throws if not found
        /// </summary>
        Task<T> FirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties);

        #endregion

        #region AutoMapper Projection Methods (DTOs - PERFORMANCE OPTIMIZED)

        /// <summary>
        /// Project entity to DTO by ID using AutoMapper - Only selects required columns
        /// </summary>
        Task<TDto?> GetByIdProjectedAsync<TDto>(TKey id, CancellationToken cancellationToken = default) where TDto : class;

        /// <summary>
        /// Project entities to DTOs using predicate - Only selects required columns
        /// </summary>
        Task<IReadOnlyList<TDto>> FindProjectedAsync<TDto>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where TDto : class;

        /// <summary>
        /// Project all entities to DTOs - Only selects required columns
        /// </summary>
        Task<IReadOnlyList<TDto>> GetAllProjectedAsync<TDto>(CancellationToken cancellationToken = default) where TDto : class;

        /// <summary>
        /// Project first entity to DTO using predicate - Only selects required columns
        /// </summary>
        Task<TDto?> FirstOrDefaultProjectedAsync<TDto>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where TDto : class;

        #endregion
    }

    /// <summary>
    /// Convenience interface for entities with int keys
    /// </summary>
    /// <typeparam name="T">The domain entity type</typeparam>
    public interface IQueryRepository<T> : IQueryRepository<T, int> where T : BaseEntity<int>
    {
    }
}