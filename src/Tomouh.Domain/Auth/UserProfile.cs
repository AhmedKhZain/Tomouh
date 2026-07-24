using Common.BaseTypes;

namespace Tomouh.Domain.Auth;

public class UserProfile : AuditableEntity<Role>
{
    public Role Role { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }
    public List<string> Permissions { get; private set; }

    public override Role Id { get => Role; }
    internal UserProfile(Role role = null, Dictionary<string, string> metadata = null, List<string> permissions = null, Guid? createdBy = null)
        : base(role, createdBy)
    {
        Role = role ?? Role.User;
        Metadata = metadata ?? new Dictionary<string, string>();
        Permissions = permissions ?? Role.Default;
    }

    internal void AddOrUpdateMetadata(string key, string value)
    {
        Metadata[key] = value;
        MarkUpdated();
    }

    internal bool RemoveMetadata(string key)
    {
        if (Metadata.Remove(key))
        {
            MarkUpdated();
            return true;
        }
        return false;
    }

    internal bool HasPermission(string permission) => Permissions.Contains(permission);

    internal void GrantPermission(string permission)
    {
        Permissions.Add(permission);
        MarkUpdated();
    }

    internal void RevokePermission(string permission)
    {
        Permissions.Remove(permission);
        MarkUpdated();
    }


}