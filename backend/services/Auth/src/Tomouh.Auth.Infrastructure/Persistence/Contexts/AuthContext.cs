using MongoDB.Driver;
using System.Linq.Expressions;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Shared.Kernel.BaseTypes;
using Tomouh.Shared.Kernel.Features;

namespace Tomouh.Auth.Infrastructure.Persistence.Contexts;

/// <summary>
/// Provides MongoDB database access, aggregate tracking, and <see cref="IUnitOfWork"/> implementation 
/// specifically tailored for the Auth Microservice.
/// </summary>
public class AuthContext : IUnitOfWork, IDisposable, IAsyncDisposable
{
    private readonly IMongoDatabase _database;
    private IClientSessionHandle? _session;

    private const string UsersCollectionName = "Users";

    /// <summary>
    /// Internal collection of tracked aggregates using reference equality to guarantee safe memory tracking.
    /// </summary>
    private readonly HashSet<IAggregate> _trackedEntities = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthContext"/> class.
    /// </summary>
    /// <param name="database">The MongoDB database interface injected from DI.</param>
    public AuthContext(IMongoDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Gets the MongoDB collection dedicated to <see cref="User"/> aggregates.
    /// </summary>
    public IMongoCollection<User> Users => _database.GetCollection<User>(UsersCollectionName);

    /// <summary>
    /// Gets the active MongoDB client session handle for transactional operations across repositories.
    /// </summary>
    public IClientSessionHandle? Session => _session;



    #region Auto-Session Transaction Wrappers

    /// <summary>
    /// Executes InsertOne automatically attaching the active transaction session if available.
    /// </summary>
    public async Task InsertOneAsync<TDocument>(IMongoCollection<TDocument> collection, TDocument document, CancellationToken cancellationToken = default)
    {
        if (_session is not null && _session.IsInTransaction)
        {
            await collection.InsertOneAsync(_session, document, cancellationToken: cancellationToken);
        }
        else
        {
            await collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Executes ReplaceOne automatically attaching the active transaction session if available.
    /// </summary>
    public async Task<ReplaceOneResult> ReplaceOneAsync<TDocument>(IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter, TDocument replacement, CancellationToken cancellationToken = default)
    {
        if (_session is not null && _session.IsInTransaction)
        {
            return await collection.ReplaceOneAsync(_session, filter, replacement, cancellationToken: cancellationToken);
        }

        return await collection.ReplaceOneAsync(filter, replacement, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Executes Find FirstOrDefault automatically attaching the active transaction session if available.
    /// </summary>
    public async Task<TDocument?> FirstOrDefaultAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        Expression<Func<TDocument, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        if (_session is not null && _session.IsInTransaction)
        {
            return await collection.Find(_session, filter).FirstOrDefaultAsync(cancellationToken);
        }

        return await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }



    /// <summary>
    /// Executes a paginated search query on a MongoDB collection, respecting active transaction sessions.
    /// </summary>
    public async Task<(List<TDocument> Items, long TotalCount)> GetPagedAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> filter,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var skip = pageIndex * pageSize;

        if (_session is not null && _session.IsInTransaction)
        {
            var totalCountTask = collection.CountDocumentsAsync(_session, filter, cancellationToken: cancellationToken);
            var itemsTask = collection.Find(_session, filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            await Task.WhenAll(totalCountTask, itemsTask);
            return (await itemsTask, await totalCountTask);
        }
        else
        {
            var totalCountTask = collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            var itemsTask = collection.Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            await Task.WhenAll(totalCountTask, itemsTask);
            return (await itemsTask, await totalCountTask);
        }
    }

    #endregion




    #region Aggregate Tracking

    /// <summary>
    /// Registers an aggregate root to the in-memory change tracker.
    /// </summary>
    /// <typeparam name="TEntity">The aggregate root type implementing <see cref="IAggregate"/>.</typeparam>
    /// <param name="aggregate">The aggregate instance to track.</param>
    /// <param name="replaceExisting">If <c>true</c>, replaces any existing tracked instance sharing the same identifier in memory.</param>
    /// <returns><c>true</c> if the aggregate was newly added to the tracker; otherwise, <c>false</c>.</returns>
    public bool TrackAggregate<TEntity>(TEntity aggregate, bool replaceExisting = false)
        where TEntity : class, IAggregate
    {
        if (aggregate is null)
        {
            return false;
        }

        if (aggregate is IEntity entity)
        {
            var currentId = entity.GetId();

            var existingTracked = _trackedEntities.FirstOrDefault(e =>
                e is IEntity trackedEntity && Equals(trackedEntity.GetId(), currentId));

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
        }

        return _trackedEntities.Add(aggregate);
    }

    /// <summary>
    /// Determines whether the specified aggregate is currently tracked within the session.
    /// </summary>
    /// <param name="aggregate">The aggregate root instance to verify.</param>
    /// <returns><c>true</c> if the aggregate is tracked; otherwise, <c>false</c>.</returns>
    public bool IsTracked(IAggregate aggregate) => _trackedEntities.Contains(aggregate);

    /// <summary>
    /// Removes an aggregate from the current change tracker session.
    /// </summary>
    /// <param name="aggregate">The aggregate root instance to stop tracking.</param>
    /// <returns><c>true</c> if the aggregate was removed; otherwise, <c>false</c>.</returns>
    public bool RemoveEntity(IAggregate aggregate) => _trackedEntities.Remove(aggregate);

    /// <summary>
    /// Retrieves a tracked entity instance matching the specified identifier type and value from memory.
    /// </summary>
    /// <typeparam name="T">The entity type implementing <see cref="IEntity{TId}"/>.</typeparam>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    /// <param name="id">The unique identifier value of the target entity.</param>
    /// <returns>The tracked entity instance if found; otherwise, <c>null</c>.</returns>
    public T? GetTrackedEntity<T, TId>(TId id) where T : class, IEntity<TId>
    {
        var comparer = EqualityComparer<TId>.Default;
        return _trackedEntities
            .OfType<T>()
            .FirstOrDefault(e => comparer.Equals(e.Id, id));
    }

    /// <summary>
    /// Retrieves all currently tracked aggregates of a specific type.
    /// </summary>
    /// <typeparam name="T">The target aggregate type implementing <see cref="IAggregate"/>.</typeparam>
    /// <returns>An enumerable sequence of tracked aggregate instances matching type <typeparamref name="T"/>.</returns>
    public IEnumerable<T> GetTrackedEntities<T>() where T : class, IAggregate
    {
        return _trackedEntities.OfType<T>();
    }

    #endregion

    #region Unit Of Work Implementation

    /// <summary>
    /// Starts a new MongoDB transaction session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the session to start.</param>
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

    /// <summary>
    /// Commits the active MongoDB transaction session and clears in-memory tracking.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the transaction commit.</param>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session is not null && _session.IsInTransaction)
        {
            await _session.CommitTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }

        _trackedEntities.Clear();
    }

    /// <summary>
    /// Aborts and rolls back the active MongoDB transaction session and clears in-memory tracking.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the transaction abort.</param>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session is not null && _session.IsInTransaction)
        {
            await _session.AbortTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }

        _trackedEntities.Clear();
    }

    /// <summary>
    /// Checks whether any tracked aggregate contains pending domain or integration events.
    /// </summary>
    /// <returns><c>true</c> if there are unhandled events; otherwise, <c>false</c>.</returns>
    public bool AnyEvents()
    {
        return _trackedEntities.Any(a =>
            (a.DomainEvents != null && a.DomainEvents.Count > 0) ||
            (a.IntegrationEvents != null && a.IntegrationEvents.Count > 0));
    }

    /// <summary>
    /// Dequeues and collects all pending Domain Events across all tracked aggregates, clearing their internal queues.
    /// </summary>
    /// <returns>A read-only list of collected <see cref="IDomainEvent"/> instances.</returns>
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

    /// <summary>
    /// Dequeues and collects all pending Integration Events across all tracked aggregates, clearing their internal queues.
    /// </summary>
    /// <returns>A read-only list of collected <see cref="IIntegrationEvent"/> instances.</returns>
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

    /// <summary>
    /// Disposes the underlying MongoDB session handle and releases unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _session?.Dispose();
        _session = null;
        _trackedEntities.Clear();
    }

    /// <summary>
    /// Asynchronously disposes the underlying MongoDB session handle and releases unmanaged resources.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_session is not null)
        {
            _session.Dispose();
            _session = null;
        }
        _trackedEntities.Clear();
        await Task.CompletedTask;
    }

    #endregion
}