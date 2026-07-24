namespace Tomouh.Domain.Auth.Repositories;

public interface IUserTokenRepository
{
    /// <summary>
    /// Fetches a single UserToken based on search criteria.
    /// </summary>
    Task<UserToken?> GetAsync(
        string? tokenHash = null,
        Guid? userId = null,
        TokenType? tokenType = null,
        bool? isUsed = null,
        bool? isRevoked = null,
        bool includeExpired = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches a list of UserTokens (useful for revoking all sessions/tokens for a user or cleanup background jobs).
    /// </summary>
    Task<IReadOnlyList<UserToken>> GetListAsync(
        Guid? userId = null,
        TokenType? tokenType = null,
        bool? isUsed = null,
        bool? isRevoked = null,
        bool includeExpired = false,
        CancellationToken cancellationToken = default);
}