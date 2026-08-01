using System.Text.Json.Serialization;
using Tomouh.Domain.Auth;

namespace Tomouh.Application.Auth.Common;

[JsonPolymorphic]
[JsonDerivedType(typeof(TFANeededAuthenticationResult), "tfa")]
[JsonDerivedType(typeof(FullAuthenticationResult), "full")]
public class AuthenticationResult
{
    public Guid UserId { get; init; }
    public bool Is2FARequired { get; init; }
    public string Name { get; init; }
    public string ShowName { get; init; }
    public bool IsActive { get; init; }
    public bool IsBlocked { get; init; }
    public List<string> RolesNames { get; init; }
    public string? Massege { get; init; } = null;

    public AuthenticationResult(User user, string? massege = null)
    {
        UserId = user.Id;
        Is2FARequired = user.TFA.IsTFAEnabled;
        Name = user.FullName;
        ShowName = user.ShowName;
        IsActive = user.Status.IsActive;
        IsBlocked = user.Status.IsBlocked;
        RolesNames = user.Profiles.Select(p => p.Role.Name).ToList();
        Massege = massege;
    }

}