using Common.ResultOf;
using Common.Services;

namespace Tomouh.Domain.Auth;

public class UserToken
{
    public Guid Id { get; private set; }
    public string TokenHash { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; }
    public TokenType TokenType { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public TokenRevokeCause? RevokeCause { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private UserToken() { }

    public static ResultOf<UserToken> Create(
        Guid userId,
        TokenType tokenType,
        ITokenHasher hasher,
        out string plainToken)
    {
        plainToken = tokenType.Create();
        var hashResult = hasher.Hash(plainToken);

        if (hashResult.IsFailure)
        {
            return hashResult.Errors;
        }

        var tokenEntity = new UserToken
        {
            Id = Guid.NewGuid(),
            TokenHash = hashResult.Value,
            UserId = userId,
            TokenType = tokenType,
            IsUsed = false,
            UsedAt = null,
            IsRevoked = false,
            RevokeCause = null,
            RevokedAt = null,
            CreatedAt = DateTime.UtcNow
        };

        return tokenEntity;
    }

    public bool IsExpired => CreatedAt.Add(TokenType.Expiration) < DateTime.UtcNow;

    public ResultOf<Done> MarkUsed(string token, ITokenHasher hasher)
    {
        if (IsUsed)
            return UserErrors.AlreadyUsedToken;

        var verifyResult = hasher.Verify(token, TokenHash);
        if (verifyResult.IsFailure || !verifyResult.Value)
            return UserErrors.InvalidToken;

        if (IsRevoked)
            return UserErrors.RevokedToken;

        if (IsExpired)
            return UserErrors.ExpiredToken;

        IsUsed = true;
        UsedAt = DateTime.UtcNow;

        return Done.Default;
    }

    public void Revoke(TokenRevokeCause cause = TokenRevokeCause.NotDetermine)
    {
        RevokeCause = cause;
        IsUsed = false;
        UsedAt = null;
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}