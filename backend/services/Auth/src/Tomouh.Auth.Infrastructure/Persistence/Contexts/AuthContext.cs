using MongoDB.Driver;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Shared.Kernel.BaseTypes;
using Tomouh.Shared.Kernel.Features;
using Tomouh.Shared.Kernel.Outbox;

namespace Tomouh.Auth.Infrastructure.Persistence.Contexts;

public class AuthContext : IUnitOfWork, IDisposable, IAsyncDisposable
{
    private readonly IMongoDatabase _database;
    private IClientSessionHandle? _session;
    private bool _disposed;

    private const string UsersCollectionName = "Users";
    private const string UserTokensCollectionName = "UserTokens";
    private const string NotificationLogsCollectionName = "NotificationLogs";

    private readonly HashSet<User> _trackedEntities = new(ReferenceEqualityComparer.Instance);

    public AuthContext(IMongoDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>(UsersCollectionName);

    public IMongoCollection<UserToken> UserTokens => _database.GetCollection<UserToken>(UserTokensCollectionName);

    public IMongoCollection<EventOutbox> NotificationLogs => _database.GetCollection<EventOutbox>(NotificationLogsCollectionName);

    public IClientSessionHandle? Session => _session;

    #region Auto-Session Transaction Wrappers

    public async Task InsertOneAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        TDocument document,
        CancellationToken cancellationToken = default)
    {
        if (_session is { IsInTransaction: true })
        {
            await collection.InsertOneAsync(_session, document, cancellationToken: cancellationToken);
        }
        else
        {
            await collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
    }

    public async Task<ReplaceOneResult> ReplaceOneAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> filter,
        TDocument replacement,
        CancellationToken cancellationToken = default)
    {
        if (_session is { IsInTransaction: true })
        {
            return await collection.ReplaceOneAsync(_session, filter, replacement, cancellationToken: cancellationToken);
        }

        return await collection.ReplaceOneAsync(filter, replacement, cancellationToken: cancellationToken);
    }

    public async Task<TDocument?> FirstOrDefaultAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> filter,
        CancellationToken cancellationToken = default)
    {
        if (_session is { IsInTransaction: true })
        {
            return await collection.Find(_session, filter).FirstOrDefaultAsync(cancellationToken);
        }

        return await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(List<TDocument> Items, long TotalCount)> GetPagedAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> filter,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var skip = pageIndex * pageSize;

        if (_session is { IsInTransaction: true })
        {
            var totalCount = await collection.CountDocumentsAsync(_session, filter, cancellationToken: cancellationToken);
            var items = await collection.Find(_session, filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
        else
        {
            var totalCount = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            var items = await collection.Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }

    #endregion

    #region Aggregate Tracking

    public bool TrackAggregate(User aggregate, bool replaceExisting = false)
    {
        if (aggregate is null)
        {
            return false;
        }

        var existingTracked = _trackedEntities.FirstOrDefault(e => e.Id == aggregate.Id);

        if (existingTracked is not null)
        {
            if (ReferenceEquals(existingTracked, aggregate))
            {
                return false;
            }

            if (replaceExisting)
            {
                _trackedEntities.Remove(existingTracked);
                return _trackedEntities.Add(aggregate);
            }

            return false;
        }

        return _trackedEntities.Add(aggregate);
    }

    public bool IsTracked(User aggregate) => _trackedEntities.Contains(aggregate);

    public bool RemoveEntity(User aggregate) => _trackedEntities.Remove(aggregate);

    public User? GetTrackedEntity(Guid id)
    {
        return _trackedEntities.FirstOrDefault(e => e.Id == id);
    }

    public IEnumerable<User> GetTrackedEntities()
    {
        return _trackedEntities;
    }

    public async Task<List<TDocument>> FindListAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> filter,
        int? limit = null,
        SortDefinition<TDocument>? sort = null,
        CancellationToken cancellationToken = default)
    {
        var findFluent = _session is { IsInTransaction: true }
            ? collection.Find(_session, filter)
            : collection.Find(filter);

        if (sort is not null)
        {
            findFluent = findFluent.Sort(sort);
        }

        if (limit.HasValue)
        {
            findFluent = findFluent.Limit(limit.Value);
        }

        return await findFluent.ToListAsync(cancellationToken);
    }

    public async Task InsertManyAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        IEnumerable<TDocument> documents,
        CancellationToken cancellationToken = default)
    {
        if (_session is { IsInTransaction: true })
        {
            await collection.InsertManyAsync(_session, documents, cancellationToken: cancellationToken);
        }
        else
        {
            await collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
        }
    }

    #endregion

    #region Unit Of Work Implementation

    public async Task StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session is not null)
        {
            return;
        }

        var client = _database.Client;
        _session = await client.StartSessionAsync(cancellationToken: cancellationToken);
        _session.StartTransaction();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session is { IsInTransaction: true })
        {
            await _session.CommitTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }

        _trackedEntities.Clear();
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session is { IsInTransaction: true })
        {
            await _session.AbortTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }

        _trackedEntities.Clear();
    }

    public bool AnyEvents()
    {
        return _trackedEntities.Any(a =>
            (a.DomainEvents != null && a.DomainEvents.Count > 0) ||
            (a.IntegrationEvents != null && a.IntegrationEvents.Count > 0));
    }

    public IReadOnlyList<IDomainEvent> CollectDomainEvents()
    {
        var domainEvents = new List<IDomainEvent>();

        foreach (var aggregate in _trackedEntities)
        {
            var events = aggregate.DequeueDomainEvents();
            if (events is { Count: > 0 })
            {
                domainEvents.AddRange(events);
            }
        }

        return domainEvents.AsReadOnly();
    }

    public IReadOnlyList<IIntegrationEvent> CollectIntegrationEvents()
    {
        var integrationEvents = new List<IIntegrationEvent>();

        foreach (var aggregate in _trackedEntities)
        {
            var events = aggregate.DequeueIntegrationEvents();
            if (events is { Count: > 0 })
            {
                integrationEvents.AddRange(events);
            }
        }

        return integrationEvents.AsReadOnly();
    }

    #endregion

    #region Resource Cleanup

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _session?.Dispose();
                _session = null;
                _trackedEntities.Clear();
            }

            _disposed = true;
        }
    }

    protected virtual ValueTask DisposeAsyncCore()
    {
        if (_session is not null)
        {
            _session.Dispose();
            _session = null;
        }

        _trackedEntities.Clear();
        return ValueTask.CompletedTask;
    }

    #endregion
}
