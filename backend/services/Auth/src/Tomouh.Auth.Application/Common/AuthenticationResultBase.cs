using System.Text.Json.Serialization;
using Tomouh.Auth.Domain.Entities;

namespace Tomouh.Auth.Application.Common;

[JsonPolymorphic]
[JsonDerivedType(typeof(TFANeededAuthenticationResult), "tfa")]
[JsonDerivedType(typeof(FullAuthenticationResult), "full")]
public class AuthenticationResult
{
    public Guid UserId { get; private set; }
    public bool Is2FARequired { get; private set; }
    public string Name { get; private set; }
    public string ShowName { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsBlocked { get; private set; }
    public List<string> RolesNames { get; private set; }
    public string? Massege { get; private set; } = null;

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
    private AuthenticationResult()
    {

    }

}