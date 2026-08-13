using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.Entities;

public class UserProfile : AuditableEntity<Role>
{
    public Role Role { get; private set; }

    private readonly Dictionary<string, string> _metadata = new();
    public IReadOnlyDictionary<string, string> Metadata => _metadata.AsReadOnly();

    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<string> Permissions => _permissions;

    public override Role Id => Role;

    // Factory/Domain Constructor
    internal UserProfile(Role role, Guid? createdBy = null)
        : base(role ?? Role.User, createdBy)
    {
        Role = role ?? Role.User;
        _metadata = new Dictionary<string, string>();

        // إعطاء الـ Default Permissions للـ Role الجديد
        _permissions = new HashSet<string>(Role.Default, StringComparer.OrdinalIgnoreCase);
    }

    // Constructor مخصص لـ MongoDB & System.Text.Json Deserialization
    [BsonConstructor]
    [JsonConstructor]
    private UserProfile(
        Role role,
        Dictionary<string, string>? metadata,
        IEnumerable<string>? permissions,
        DateTime createdAt,
        DateTime? lastUpdate,
        Guid? createdBy)
        : base(role, createdBy)
    {
        Role = role;
        _metadata = metadata ?? new Dictionary<string, string>();
        _permissions = permissions is not null
            ? new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        CreatedAt = createdAt;
        LastUpdate = lastUpdate;
    }

    internal void AddOrUpdateMetadata(string key, string value)
    {
        _metadata[key] = value;
        MarkUpdated();
    }

    internal bool RemoveMetadata(string key)
    {
        if (_metadata.Remove(key))
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
        return false; // Permission was already granted
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