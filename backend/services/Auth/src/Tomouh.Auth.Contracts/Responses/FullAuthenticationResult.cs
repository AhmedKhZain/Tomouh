using Tomouh.Auth.Domain.Entities;

namespace Tomouh.Auth.Contracts.Responses;

public class FullAuthenticationResult : AuthenticationResult
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpireAt { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpireAt { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool IsConfirmedEmail { get; init; }
    public IReadOnlyList<UserProfileResult> UserProfiles { get; init; } = [];

    public FullAuthenticationResult(User user, string token, DateTime expireAt, string refreshToken, DateTime refreshTokenExpireAt, string? message = null)
        : base(user, message)
    {
        Token = token;
        ExpireAt = expireAt;
        RefreshToken = refreshToken;
        RefreshTokenExpireAt = refreshTokenExpireAt;
        Email = user.MainEmail.Email;
        IsConfirmedEmail = user.MainEmail.IsEmailConfirmed;
        UserProfiles = user.Profiles.Select(p => new UserProfileResult
        {
            ProfileRole = p.Role.NormalizedLowerCaseName,
            MetaData = p.Metadata
        }).ToList();
    }

    protected FullAuthenticationResult() { }
}
