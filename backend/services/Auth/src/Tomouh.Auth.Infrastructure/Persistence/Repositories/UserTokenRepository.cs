using MongoDB.Driver;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Auth.Infrastructure.Persistence.Contexts;

namespace Tomouh.Auth.Infrastructure.Persistence.Repositories;

public class UserTokenRepository : IUserTokenRepository
{
    private readonly AuthContext _context;

    public UserTokenRepository(AuthContext context)
    {
        _context = context;
    }

    public async Task<UserToken?> GetAsync(
        string? tokenHash = null,
        Guid? userId = null,
        TokenType? tokenType = null,
        bool? isUsed = null,
        bool? isRevoked = null,
        bool includeExpired = true,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(tokenHash, userId, tokenType, isUsed, isRevoked, includeExpired);

        return await _context.FirstOrDefaultAsync(_context.UserTokens, filter, cancellationToken);
    }

    public async Task<IReadOnlyList<UserToken>> GetListAsync(
        Guid? userId = null,
        TokenType? tokenType = null,
        bool? isUsed = null,
        bool? isRevoked = null,
        bool includeExpired = false,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(null, userId, tokenType, isUsed, isRevoked, includeExpired);

        return await _context.FindListAsync(_context.UserTokens, filter, cancellationToken: cancellationToken);
    }

    public async Task AddAsync(UserToken token, CancellationToken cancellationToken = default)
    {
        await _context.InsertOneAsync(_context.UserTokens, token, cancellationToken);
    }

    public async Task UpdateAsync(UserToken token, CancellationToken cancellationToken = default)
    {
        var filter = Builders<UserToken>.Filter.Eq(t => t.Id, token.Id);
        await _context.ReplaceOneAsync(_context.UserTokens, filter, token, cancellationToken);
    }

    private FilterDefinition<UserToken> BuildFilter(
        string? tokenHash,
        Guid? userId,
        TokenType? tokenType,
        bool? isUsed,
        bool? isRevoked,
        bool includeExpired)
    {
        var builder = Builders<UserToken>.Filter;
        var filters = new List<FilterDefinition<UserToken>>();

        if (!string.IsNullOrWhiteSpace(tokenHash))
        {
            filters.Add(builder.Eq(t => t.TokenHash, tokenHash));
        }

        if (userId.HasValue)
        {
            filters.Add(builder.Eq(t => t.UserId, userId.Value));
        }

        if (tokenType is not null)
        {
            filters.Add(builder.Eq(t => t.TokenType, tokenType));
        }

        if (isUsed.HasValue)
        {
            filters.Add(builder.Eq(t => t.IsUsed, isUsed.Value));
        }

        if (isRevoked.HasValue)
        {
            filters.Add(builder.Eq(t => t.IsRevoked, isRevoked.Value));
        }

        if (!includeExpired)
        {
            var expiration = tokenType?.Expiration ?? TokenType.RefreshToken.Expiration;
            filters.Add(builder.Gte(t => t.CreatedAt, DateTime.UtcNow.Subtract(expiration)));
        }

        return filters.Count > 0 ? builder.And(filters) : builder.Empty;
    }
}
