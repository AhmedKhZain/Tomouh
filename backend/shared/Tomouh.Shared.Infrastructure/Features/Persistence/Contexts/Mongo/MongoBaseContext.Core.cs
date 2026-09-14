using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tomouh.Shared.Kernel.BaseTypes;
using Tomouh.Shared.Kernel.Features;
using Tomouh.Shared.Kernel.Outbox;

namespace Tomouh.Shared.Infrastructure.Features.Persistence.Contexts.Mongo;

/// <summary>
/// Represents the abstract base context for MongoDB persistence, supporting transactions, automatic tracking, and LINQ queries.
/// </summary>
public abstract partial class MongoBaseContext : IUnitOfWork, IDisposable, IAsyncDisposable
{
    protected readonly IMongoDatabase Database;
    private IClientSessionHandle? _session;
    private bool _disposed;

    protected readonly HashSet<IAggregate> TrackedEntities;
    public string NotificationLogsCollectionName => GetCollectionName("NotificationLogs");

    public IMongoCollection<EventOutbox> NotificationLogs => Database.GetCollection<EventOutbox>(NotificationLogsCollectionName);

    protected MongoBaseContext(IMongoDatabase database)
    {
        Database = database ?? throw new ArgumentNullException(nameof(database));
        TrackedEntities = new(ReferenceEqualityComparer.Instance);
    }

    public IClientSessionHandle? Session => _session;

    /// <summary>
    /// Generates a convention-based collection name using the context name as a prefix
    /// to isolate collections between different services or modules sharing the same database.
    /// </summary>
    protected virtual string GetCollectionName(string entityName)
    {
        var contextName = GetType().Name;
        if (contextName.Contains("Mongo", StringComparison.OrdinalIgnoreCase))
            contextName = contextName.Replace("Mongo", string.Empty, StringComparison.OrdinalIgnoreCase);

        string collectionPrefix = null;
        if (contextName.Contains("Context", StringComparison.OrdinalIgnoreCase))
            collectionPrefix = contextName.Replace("Context", string.Empty);
        else
            collectionPrefix = contextName;

        // لو حابب تفصل بينهم بنقطة (Auth.Users) أو شرطة (Auth_Users)
        return $"{collectionPrefix}.{entityName}";
    }

    #region Generic Data Operations & Auto-Tracking

    /// <summary>
    /// Inserts a single document into the specified collection with optional automatic aggregate tracking.
    /// </summary>
    public async Task InsertOneAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        TDocument document,
        bool track = true,
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

        if (track && document is IAggregate aggregate)
        {
            TrackAggregate(aggregate);
        }
    }

    /// <summary>
    /// Inserts multiple documents into the specified collection with optional automatic tracking.
    /// </summary>
    public async Task InsertManyAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        IEnumerable<TDocument> documents,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        var list = documents.ToList();

        if (_session is { IsInTransaction: true })
        {
            await collection.InsertManyAsync(_session, list, cancellationToken: cancellationToken);
        }
        else
        {
            await collection.InsertManyAsync(list, cancellationToken: cancellationToken);
        }

        if (track)
        {
            var aggregates = list.OfType<IAggregate>();
            TrackAggregates(aggregates);
        }
    }

    /// <summary>
    /// Replaces a single document matching the filter with a new replacement document.
    /// </summary>
    public async Task ReplaceAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> filter,
        TDocument replacement,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        if (_session is { IsInTransaction: true })
        {
            await collection.ReplaceOneAsync(_session, filter, replacement, cancellationToken: cancellationToken);
        }
        else
        {
            await collection.ReplaceOneAsync(filter, replacement, cancellationToken: cancellationToken);
        }

        if (track && replacement is IAggregate aggregate)
        {
            TrackAggregate(aggregate);
        }
    }

    /// <summary>
    /// Performs a bulk replacement operation for multiple documents using bulk write commands.
    /// </summary>
    public async Task ReplaceManyAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        IEnumerable<(FilterDefinition<TDocument> Filter, TDocument Replacement)> replacements,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        var list = replacements.ToList();
        if (list.Count == 0) return;

        var models = list.Select(r => new ReplaceOneModel<TDocument>(r.Filter, r.Replacement)
        {
            IsUpsert = false
        }).ToList();

        if (_session is { IsInTransaction: true })
        {
            await collection.BulkWriteAsync(_session, models, cancellationToken: cancellationToken);
        }
        else
        {
            await collection.BulkWriteAsync(models, cancellationToken: cancellationToken);
        }

        if (track)
        {
            var aggregates = list.Select(r => r.Replacement).OfType<IAggregate>();
            TrackAggregates(aggregates);
        }
    }

    /// <summary>
    /// Finds the first document matching the filter criteria.
    /// </summary>
    public async Task<TDocument?> FirstOrDefaultAsync<TDocument>(
        IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> filter,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        var document = _session is { IsInTransaction: true }
            ? await collection.Find(_session, filter).FirstOrDefaultAsync(cancellationToken)
            : await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (track && document is IAggregate aggregate)
        {
            TrackAggregate(aggregate);
        }

        return document;
    }

    /// <summary>
    /// Queries collection data using LINQ with projection support and optional automatic entity tracking.
    /// </summary>
    public async Task<List<TResult>> FindWithLinqAsync<TDocument, TResult>(
        IMongoCollection<TDocument> collection,
        Func<IQueryable<TDocument>, IQueryable<TResult>> queryBuilder,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = _session is { IsInTransaction: true }
            ? collection.AsQueryable(_session)
            : collection.AsQueryable();

        var builtQuery = queryBuilder(queryable);
        var items = await builtQuery.ToListAsync(cancellationToken);

        if (track)
        {
            foreach (var item in items)
            {
                if (item is IAggregate aggregate)
                {
                    TrackAggregate(aggregate);
                }
            }
        }

        return items;
    }

    /// <summary>
    /// Retrieves paged data using LINQ and returns a clean tuple containing the items list and total count separately.
    /// </summary>
    public async Task<(List<TResult> Items, long TotalCount)> GetPagedWithLinqAsync<TDocument, TResult>(
        IMongoCollection<TDocument> collection,
        Func<IQueryable<TDocument>, IQueryable<TResult>> queryBuilder,
        int pageIndex,
        int pageSize,
        FilterDefinition<TDocument>? countFilter = null,
        bool track = false,
        CancellationToken cancellationToken = default)
    {
        var skip = pageIndex * pageSize;

        long totalCount = 0;
        if (countFilter is not null)
        {
            totalCount = _session is { IsInTransaction: true }
                ? await collection.CountDocumentsAsync(_session, countFilter, cancellationToken: cancellationToken)
                : await collection.CountDocumentsAsync(countFilter, cancellationToken: cancellationToken);
        }

        var queryable = _session is { IsInTransaction: true }
            ? collection.AsQueryable(_session)
            : collection.AsQueryable();

        var builtQuery = queryBuilder(queryable);

        if (countFilter is null)
        {
            totalCount = await builtQuery.LongCountAsync(cancellationToken);
        }

        var items = await builtQuery
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (track)
        {
            foreach (var item in items)
            {
                if (item is IAggregate aggregate)
                {
                    TrackAggregate(aggregate);
                }
            }
        }

        return (items, totalCount);
    }

    #endregion
}