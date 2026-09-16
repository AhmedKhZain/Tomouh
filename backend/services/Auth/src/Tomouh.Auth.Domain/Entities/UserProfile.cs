using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.ValueObjects;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.Entities;

public class UserProfile : AuditableEntity<Role>
{
public Role Role { get; private set; }

    private HashSet<AccountMetadata> _metadata = new();
    [JsonPropertyName("metadata")]
    public IReadOnlyCollection<AccountMetadata> Metadata => _metadata;

    private HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);
    [JsonPropertyName("permissions")]
    public IReadOnlyCollection<string> Permissions => _permissions;

    public override Role Id => Role;

    // Factory/Domain Constructor
    internal UserProfile(Role role, Guid? createdBy = null)
        : base(role ?? Role.User, createdBy)
    {
        Role = role ?? Role.User;

        _permissions = new HashSet<string>(Role.Default, StringComparer.OrdinalIgnoreCase);
    }


    // Constructor for MongoDB & System.Text.Json Deserialization
    [BsonConstructor]
    [JsonConstructor]
    private UserProfile(
        Role role,
        IReadOnlyCollection<AccountMetadata>? metadata,
        IReadOnlyCollection<string>? permissions,
        DateTime createdAt,
        DateTime? lastUpdate,
        Guid? createdBy)
        : base(role, createdBy)
    {
        Role = role;
        _metadata = metadata is null ? new HashSet<AccountMetadata>() : new HashSet<AccountMetadata>(metadata);
        _permissions = permissions is null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);

        CreatedAt = createdAt;
        LastUpdate = lastUpdate;
        CreatedBy = createdBy;
    }

    internal void AddOrUpdateMetadata(string key, string value, AccountMetadataType type, bool isPublic)
    {
        var existing = _metadata.FirstOrDefault(m => m.Key == key);

        if (existing is not null)
        {
            var updated = existing.WithValue(value, isPublic, type);
            _metadata.Remove(existing);
            _metadata.Add(updated);
        }
        else
        {
            _metadata.Add(new AccountMetadata(key, value, isPublic, DateTime.UtcNow, type));
        }

        MarkUpdated();
    }

    internal bool RemoveMetadata(string key)
    {
        var existing = _metadata.FirstOrDefault(m => m.Key == key);

        if (existing is not null && _metadata.Remove(existing))
        {
            MarkUpdated();
            return true;
        }
        return false;
    }

    internal bool HasPermission(string permission) => _permissions.Contains(permission);

    internal bool GrantPermission(string permission)
    {
        if (_permissions.Add(permission))
        {
            MarkUpdated();
            return true;
        }
        return false;
    }

    internal bool RevokePermission(string permission)
    {
        if (_permissions.Remove(permission))
        {
            MarkUpdated();
            return true;
        }
        return false;
    }
}