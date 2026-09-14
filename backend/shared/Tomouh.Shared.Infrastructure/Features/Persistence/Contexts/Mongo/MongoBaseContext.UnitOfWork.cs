namespace Tomouh.Shared.Infrastructure.Features.Persistence.Contexts.Mongo;

public abstract partial class MongoBaseContext
{
    /// <summary>
    /// Starts a new database session and transaction.
    /// </summary>
    public async Task StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session is not null)
        {
            return;
        }

        var client = Database.Client;
        _session = await client.StartSessionAsync(cancellationToken: cancellationToken);
        _session.StartTransaction();
    }

    /// <summary>
    /// Commits the active transaction and clears all tracked entities.
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session is { IsInTransaction: true })
        {
            await _session.CommitTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }

        TrackedEntities.Clear();
    }

    /// <summary>
    /// Aborts and rolls back the active transaction, clearing tracked entities.
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session is { IsInTransaction: true })
        {
            await _session.AbortTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }

        TrackedEntities.Clear();
    }
    public bool IsInTransaction => _session?.IsInTransaction ?? false;

}