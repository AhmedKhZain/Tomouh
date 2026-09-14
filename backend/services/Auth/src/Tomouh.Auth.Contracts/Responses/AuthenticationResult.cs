using System.Text.Json.Serialization;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.ValueObjects;

namespace Tomouh.Auth.Contracts.Responses;

[JsonPolymorphic]
[JsonDerivedType(typeof(TFANeededAuthenticationResult), "tfa")]
[JsonDerivedType(typeof(FullAuthenticationResult), "full")]
public class AuthenticationResult
{
    public Guid UserId { get; private set; }
    public bool Is2FARequired { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ShowName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool IsBlocked { get; private set; }
    public List<string> RolesNames { get; private set; } = [];
    public IReadOnlyCollection<AccountMetadata> MetaData { get; private set; } = [];
    public string? Message { get; private set; }

    public AuthenticationResult(User user, string? message = null)
    {
        UserId = user.Id;
        Is2FARequired = user.TFA.IsTFAEnabled;
        Name = user.FullName;
        ShowName = user.ShowName;
        IsActive = user.Status.IsActive;
        IsBlocked = user.Status.IsBlocked;
        RolesNames = user.Profiles.Select(p => p.Role.Name).ToList();
        MetaData = user.Metadata;
        Message = message;
    }

    protected AuthenticationResult() { }
}
