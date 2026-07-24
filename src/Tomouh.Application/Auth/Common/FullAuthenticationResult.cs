using Tomouh.Domain.Auth;

namespace Tomouh.Application.Auth.Common;

public class FullAuthenticationResult : AuthenticationResultBase
{
    public string Token { get; init; }
    public string RefreshToken { get; init; }
    public string Email { get; init; }
    public bool IsConfirmedEmail { get; init; }
    public IReadOnlyList<UserProfileAuthResult> UserProfiles { get; init; }
    public FullAuthenticationResult(User user, string token, string refreshToken, string? massege = null) : base(user, massege)
    {
        Token = token;
        RefreshToken = refreshToken;
        Email = user.Email.Email;
        IsConfirmedEmail = user.Email.IsEmailConfirmed;
        UserProfiles = user.Profiles.Select(p => new UserProfileAuthResult
        {
            ProfileRole = p.Role.NormalizedLowerCaseName,
            MetaData = p.Metadata
        }).ToList();
    }
}
