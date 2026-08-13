using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Application.Interfaces;

public interface IMongoEntitiesTracker
{
    bool IsTracked(IAggregate aggregate);
    bool TrackAggregate<TEntity>(TEntity aggregate, bool replaceExisting = false) where TEntity : class, IAggregate;
    bool RemoveEntity(IAggregate aggregate);
    T? GetTrackedEntity<T, TId>(TId id) where T : class, IEntity<TId>;
    IEnumerable<T> GetTrackedEntities<T>() where T : class, IAggregate;

}
