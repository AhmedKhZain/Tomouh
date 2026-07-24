namespace Common.BaseTypes;

/// <summary>
/// Abstract base class for all entities in the system.
/// Provides base properties like Id, CreatedAt, and CreatedBy.
/// </summary>
public abstract class BaseEntity<TId> : IEntity<TId>
{
    public virtual TId Id { get; protected set; } = default!;
    public Guid? CreatedBy { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }
    protected BaseEntity(Guid? createdBy = null)
    {
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    protected BaseEntity(TId id, Guid? createdBy = null) : this(createdBy)
    {
        Id = id;
    }

    protected BaseEntity() { }

    /// <summary>
    /// Sets the creator if it has not been set yet.
    /// </summary>
    public void SetCreator(Guid creatorId)
    {
        if (CreatedBy is not null) return;
        CreatedBy = creatorId;
    }


    /// <summary>
    /// Determines whether the specified object is equal to the current entity based on entity type and key equality.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns><c>true</c> if the specified object is an entity of the same type with an identical identifier; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (EqualityComparer<TId>.Default.Equals(Id, default!) || EqualityComparer<TId>.Default.Equals(other.Id, default!))
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Serves as the default hash function using the entity identifier.
    /// </summary>
    /// <returns>A hash code for the current entity based on its identifier and runtime type.</returns>
    public override int GetHashCode()
    {
        if (EqualityComparer<TId>.Default.Equals(Id, default!))
            return base.GetHashCode();

        return HashCode.Combine(GetType(), Id);
    }

    /// <summary>
    /// Compares two entity instances for equality using entity identity semantics.
    /// </summary>
    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {

        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Compares two entity instances for inequality using entity identity semantics.
    /// </summary>
    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        return !(left == right);
    }

}
