using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Common.BaseTypes;

/// <summary>
/// Extends BaseEntity to add audit tracking for update operations.
/// </summary>
public abstract class AuditableEntity<TId> : BaseEntity<TId>, IAuditable<TId>
{
    [BsonIgnore]
    [JsonIgnore]
    public bool IsUpdated { get; protected set; } = false;
    public DateTime? LastUpdate { get; protected set; }
    [JsonIgnore]
    object IAuditable.Id => Id!;

    protected AuditableEntity(TId id, Guid? createdBy = null) : base(id, createdBy) { }
    protected AuditableEntity(Guid? createdBy = null) : base(createdBy) { }
    protected AuditableEntity() : base() { }

    /// <summary>
    /// Updates the LastUpdate timestamp to the current UTC time.
    /// </summary>
    protected void UpdateTimestamp() => LastUpdate = DateTime.UtcNow;

    public void MarkUpdated()
    {
        UpdateTimestamp();
        IsUpdated = true;
    }

}