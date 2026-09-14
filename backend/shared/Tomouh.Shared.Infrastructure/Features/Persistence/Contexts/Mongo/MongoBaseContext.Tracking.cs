using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Shared.Infrastructure.Features.Persistence.Contexts.Mongo;

public abstract partial class MongoBaseContext
{
    /// <summary>
    /// Tracks an aggregate entity for domain and integration event dispatching upon commit.
    /// </summary>
    public bool TrackAggregate(IAggregate aggregate, bool replaceExisting = false)
    {
        if (aggregate is null)
        {
            return false;
        }

        var existingTracked = TrackedEntities.FirstOrDefault(e => e.Equals(aggregate));

        if (existingTracked is not null)
        {
            if (ReferenceEquals(existingTracked, aggregate))
            {
                return false;
            }

            if (replaceExisting)
            {
                TrackedEntities.Remove(existingTracked);
                return TrackedEntities.Add(aggregate);
            }

            return false;
        }

        return TrackedEntities.Add(aggregate);
    }

    /// <summary>
    /// Tracks a collection of aggregate entities at once.
    /// </summary>
    public void TrackAggregates(IEnumerable<IAggregate> aggregates, bool replaceExisting = false)
    {
        if (aggregates is null)
        {
            return;
        }

        foreach (var aggregate in aggregates)
        {
            TrackAggregate(aggregate, replaceExisting);
        }
    }

    /// <summary>
    /// Checks if a given aggregate entity is currently being tracked.
    /// </summary>
    public bool IsTracked(IAggregate aggregate) => TrackedEntities.Contains(aggregate);

    /// <summary>
    /// Removes an aggregate entity from the tracked list.
    /// </summary>
    public bool RemoveEntity(IAggregate aggregate) => TrackedEntities.Remove(aggregate);

    /// <summary>
    /// Returns all currently tracked aggregate entities.
    /// </summary>
    public IEnumerable<IAggregate> GetTrackedEntities() => TrackedEntities;

    /// <summary>
    /// Returns all currently tracked aggregate entities of T.
    /// </summary>
    public IEnumerable<T> GetTrackedEntities<T>() => TrackedEntities.OfType<T>();
}