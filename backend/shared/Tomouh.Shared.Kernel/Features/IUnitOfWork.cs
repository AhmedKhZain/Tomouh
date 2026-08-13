using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Shared.Kernel.Features;

/// <summary>
/// Defines the core contract for unit of work pattern implementations, managing transactional 
/// boundaries and collecting aggregate events across business operations.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Starts a new database transaction session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous transaction initialization.</returns>
    Task StartTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the active database transaction session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous transaction commit.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts and rolls back the active database transaction session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous transaction rollback.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether any currently tracked aggregate contains pending domain or integration events.
    /// </summary>
    /// <returns><c>true</c> if there are unhandled events; otherwise, <c>false</c>.</returns>
    bool AnyEvents();

    /// <summary>
    /// Dequeues and collects all pending Domain Events across all tracked aggregates.
    /// </summary>
    /// <returns>A read-only list of collected <see cref="IDomainEvent"/> instances.</returns>
    IReadOnlyList<IDomainEvent> CollectDomainEvents();

    /// <summary>
    /// Dequeues and collects all pending Integration Events across all tracked aggregates.
    /// </summary>
    /// <returns>A read-only list of collected <see cref="IIntegrationEvent"/> instances.</returns>
    IReadOnlyList<IIntegrationEvent> CollectIntegrationEvents();
}