using Common.BaseTypes;

namespace Tomouh.Application.Common.Interfaces;

public interface IMongoEntitiesTracker
{
    bool IsTracked(IAggregate aggregate);
    bool TrackAggregate<TEntity>(TEntity aggregate, bool replaceExisting = false) where TEntity : class, IAggregate;
    bool RemoveEntity(IAggregate aggregate);
    T? GetTrackedEntity<T, TId>(TId id) where T : class, IEntity<TId>;
    IEnumerable<T> GetTrackedEntities<T>() where T : class, IAggregate;

}
