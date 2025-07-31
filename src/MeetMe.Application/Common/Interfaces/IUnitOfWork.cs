using MeetMe.Domain.Common;

namespace MeetMe.Application.Common.Interfaces
{
    /// <summary>
    /// Interface for managing database transactions across multiple repositories
    /// All operations are asynchronous to improve application scalability
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable
    {
        /// <summary>
        /// Gets a command repository for a specific entity type
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <returns>Command repository for the entity type</returns>
        ICommandRepository<T> CommandRepository<T>() where T : BaseEntity;

        /// <summary>
        /// Gets a command repository for a specific entity type with custom key
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <returns>Command repository for the entity type</returns>
        ICommandRepository<T, TKey> CommandRepository<T, TKey>() where T : BaseEntity<TKey> where TKey : IEquatable<TKey>;

        /// <summary>
        /// Begins a transaction asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the transaction asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the transaction asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database
        /// </summary>
        /// <param name="userId">ID of the user performing the action (for audit)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of state entries written to the database</returns>
        Task<int> SaveChangesAsync(string userId, CancellationToken cancellationToken = default);
    }
}