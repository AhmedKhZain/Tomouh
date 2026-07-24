using Common.BaseTypes;
using MongoDB.Driver;
using Tomouh.Application.Common.Interfaces;
using Tomouh.Domain.Auth;

namespace Tomouh.Infrastructure.Persistence.NoSql;

/// <summary>
/// Provides MongoDB database access, collection exposure, aggregate tracking, and domain event collection.
/// </summary>
public class TomouhMongoContext : IMongoEntitiesTracker
{
    private readonly IMongoDatabase _database;
    private readonly IDomainEventCollector _eventCollector;

    /// <summary>
    /// Internal collection of tracked aggregates using reference equality to guarantee safe memory tracking.
    /// </summary>
    private readonly HashSet<IAggregate> _trackedEntities = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Initializes a new instance of the <see cref="TomouhMongoContext"/> class.
    /// </summary>
    /// <param name="client">The MongoDB client interface.</param>
    /// <param name="eventCollector">The domain event collector service.</param>
    public TomouhMongoContext(IMongoClient client, IDomainEventCollector eventCollector)
    {
        _database = client.GetDatabase("TomouhAuthDb");
        _eventCollector = eventCollector;
    }

    /// <summary>
    /// Gets the MongoDB collection for <see cref="User"/> aggregates.
    /// </summary>
    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");


    /// <summary>
    /// Registers an aggregate root to the in-memory change tracker.
    /// /// Automatically infers the identifier type from implemented <see cref="IEntity{TId}"/> interfaces.
    /// If another aggregate instance with the same identifier exists and <paramref name="replaceExisting"/> is <c>true</c>,
    /// the old instance is replaced by the new instance in memory while retaining existing collected events.
    /// </summary>
    /// <typeparam name="TEntity">The aggregate root type.</typeparam>
    /// <param name="aggregate">The aggregate instance to track.</param>
    /// <param name="replaceExisting">If <c>true</c>, replaces any existing tracked instance sharing the same identifier in memory.</param>
    /// <returns><c>true</c> if the aggregate was newly added to the tracker; otherwise, <c>false</c>.</returns>
    public bool TrackAggregate<TEntity>(TEntity aggregate, bool replaceExisting = false)
            where TEntity : class, IAggregate
    {
        // Extract IEntity<TId> dynamically to support any Id type (Guid, string, int, ObjectId, etc.)
        var entityInterface = typeof(TEntity)
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));

        if (entityInterface is not null)
        {
            var idProperty = entityInterface.GetProperty("Id");
            var currentId = idProperty?.GetValue(aggregate);

            if (currentId is not null)
            {
                // Search for an existing tracked entity implementing the same interface and matching ID
                var existingTracked = _trackedEntities.FirstOrDefault(e =>
                {
                    if (entityInterface.IsAssignableFrom(e.GetType()))
                    {
                        var existingId = idProperty?.GetValue(e);
                        return Equals(existingId, currentId);
                    }
                    return false;
                });

                if (existingTracked is not null)
                {
                    // If exact same reference exists, do nothing
                    if (ReferenceEquals(existingTracked, aggregate))
                    {
                        return false;
                    }

                    // If a different instance exists with the same ID and replaceExisting is requested
                    if (replaceExisting)
                    {
                        _trackedEntities.Remove(existingTracked);
                        _trackedEntities.Add(aggregate);

                        _eventCollector.CollectEventsFromAggregate(aggregate);
                        return false;
                    }

                    return false;
                }
            }
        }

        // Add as brand new entity if no matching ID was found in memory
        bool isNew = _trackedEntities.Add(aggregate);
        if (isNew)
        {
            _eventCollector.CollectEventsFromAggregate(aggregate);
        }

        return isNew;
    }
    /// <summary>
    /// Determines whether the specified aggregate is currently tracked within the session.
    /// </summary>
    /// <param name="aggregate">The aggregate root instance to verify.</param>
    /// <returns><c>true</c> if the aggregate is tracked; otherwise, <c>false</c>.</returns>
    public bool IsTracked(IAggregate aggregate)
    {
        return _trackedEntities.Contains(aggregate);
    }

    /// <summary>
    /// Removes an aggregate from the current change tracker session.
    /// </summary>
    /// <param name="aggregate">The aggregate root instance to stop tracking.</param>
    /// <returns><c>true</c> if the aggregate was removed; otherwise, <c>false</c>.</returns>
    public bool RemoveEntity(IAggregate aggregate)
    {
        return _trackedEntities.Remove(aggregate);
    }

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


}