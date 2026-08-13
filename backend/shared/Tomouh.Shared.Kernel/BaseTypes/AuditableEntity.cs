using System.Text.Json.Serialization;

namespace Tomouh.Shared.Kernel.BaseTypes;

/// <summary>
/// Extends BaseEntity to add audit tracking for update operations.
/// </summary>
public abstract class AuditableEntity<TId> : BaseEntity<TId>, IAuditable<TId>
{
    public DateTime? LastUpdate { get; protected set; }
    [JsonIgnore]
    object IAuditable.Id => Id!;

    protected AuditableEntity(TId id, Guid? createdBy = null) : base(id, createdBy) { }
    protected AuditableEntity(Guid? createdBy = null) : base(createdBy) { }
    protected AuditableEntity() : base() { }

    /// <summary>
    /// Updates the LastUpdate timestamp to the current UTC time.
    /// </summary>
    public void MarkUpdated()
    {
        LastUpdate = DateTime.UtcNow;
    }

}