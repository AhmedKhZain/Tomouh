using Tomouh.Auth.Domain.Entities;

namespace Tomouh.Auth.Application.Common;

public class FullAuthenticationResult : AuthenticationResult
{
    public string Token { get; init; }
    public DateTime ExpireAt { get; init; }
    public string Email { get; init; }
    public bool IsConfirmedEmail { get; init; }
    public IReadOnlyList<UserProfileAuthResult> UserProfiles { get; init; }
    public FullAuthenticationResult(User user, string token, DateTime expireAt, string? massege = null) : base(user, massege)
    {
        Token = token;
        ExpireAt = expireAt;
        Email = user.MainEmail.Email;
        IsConfirmedEmail = user.MainEmail.IsEmailConfirmed;
        UserProfiles = user.Profiles.Select(p => new UserProfileAuthResult
        {
            ProfileRole = p.Role.NormalizedLowerCaseName,
            MetaData = p.Metadata
        }).ToList();
    }
}
