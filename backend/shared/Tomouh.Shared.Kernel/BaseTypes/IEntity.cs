using Tomouh.Shared.Kernel.Markups;

namespace Tomouh.Shared.Kernel.BaseTypes;

/// <summary>
/// Provides a non-generic access contract for entities to expose identifier objects.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier object for the entity.
    /// </summary>
    /// <returns>An object representing the entity's unique identifier.</returns>
    object GetId();

    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Tells whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; }
}

/// <summary>
/// Represents a generic base entity contract with a strongly-typed identifier.
/// </summary>
/// <typeparam name="TId">The type of the unique identifier.</typeparam>
public interface IEntity<out TId> : IEntity, IHasId<TId>, ICreatorTrackable
{
    /// <summary>
    /// Implicit default implementation satisfying the non-generic <see cref="IEntity.GetId"/> contract.
    /// </summary>
    object IEntity.GetId() => Id!;
}