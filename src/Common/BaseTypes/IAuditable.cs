namespace Common.BaseTypes;

/// <summary>
/// Defines a contract for auditing properties on entities.
/// </summary>
public interface IAuditable
{
    object Id { get; }
    DateTime CreatedAt { get; }
    bool IsUpdated { get; }
    DateTime? LastUpdate { get; }
    Guid? CreatedBy { get; }
}

/// <summary>
/// Defines a generic contract for auditing with a specific identifier type.
/// </summary>
public interface IAuditable<out TId> : IAuditable, IEntity<TId>
{
    new TId Id { get; }
}