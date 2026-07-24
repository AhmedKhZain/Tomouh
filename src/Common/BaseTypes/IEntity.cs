using Common.Markups;

namespace Common.BaseTypes;

/// <summary>
/// Represents a base entity with an ID and creation tracking.
/// </summary>
public interface IEntity<out TId> : IHasId<TId>, ICreatorTrackable
{
    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; }
    /// <summary>
    /// Tells whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; }

}