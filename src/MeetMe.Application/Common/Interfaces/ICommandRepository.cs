using MeetMe.Domain.Common;

namespace MeetMe.Application.Common.Interfaces
{
    /// <summary>
    /// Generic repository interface for write operations, following CQRS pattern
    /// All operations are asynchronous to improve application scalability
    /// </summary>
    /// <typeparam name="T">The domain entity type</typeparam>
    /// <typeparam name="TKey">The type of entity's primary key</typeparam>
    public interface ICommandRepository<T, TKey> where T : BaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Adds a new entity asynchronously
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <param name="userId">ID of the user performing the action (for audit)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The entity after it has been added</returns>
        Task<T> AddAsync(T entity, int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity asynchronously
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <param name="userId">ID of the user performing the action (for audit)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task UpdateAsync(T entity, int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity asynchronously
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity by its ID asynchronously
        /// </summary>
        /// <param name="id">ID of the entity to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if entity was found and deleted; otherwise false</returns>
        Task<bool> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft deletes an entity by marking it as inactive rather than removing it
        /// </summary>
        /// <param name="entity">Entity to soft delete</param>
        /// <param name="userId">ID of the user performing the action (for audit)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SoftDeleteAsync(T entity, int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves all changes to the database asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of state entries written to the database</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Convenience interface for entities with int keys
    /// </summary>
    /// <typeparam name="T">The domain entity type</typeparam>
    public interface ICommandRepository<T> : ICommandRepository<T, int> where T : BaseEntity<int>
    {
    }
}