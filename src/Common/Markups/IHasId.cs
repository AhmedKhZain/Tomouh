namespace Common.Markups;

/// <summary>
/// Defines a generic contract for entities with a unique identifier.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
public interface IHasId<out TId>
{
    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
    TId Id { get; }
}
