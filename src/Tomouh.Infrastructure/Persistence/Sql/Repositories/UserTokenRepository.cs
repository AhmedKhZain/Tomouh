using MongoDB.Driver.Linq;
using Tomouh.Domain.Auth;
using Tomouh.Domain.Auth.Repositories;

namespace Tomouh.Infrastructure.Persistence.Sql.Repositories
{
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly TomouhDbContext _dbContext;

        public UserTokenRepository(TomouhDbContext dbContext)
        {
            _dbContext = dbContext;
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
            var query = BuildQuery(tokenHash, userId, tokenType, isUsed, isRevoked, includeExpired);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<UserToken>> GetListAsync(
            Guid? userId = null,
            TokenType? tokenType = null,
            bool? isUsed = null,
            bool? isRevoked = null,
            bool includeExpired = false,
            CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(null, userId, tokenType, isUsed, isRevoked, includeExpired);

            return await query.ToListAsync(cancellationToken);
        }

        private IQueryable<UserToken> BuildQuery(
            string? tokenHash,
            Guid? userId,
            TokenType? tokenType,
            bool? isUsed,
            bool? isRevoked,
            bool includeExpired)
        {
            var query = _dbContext.UserTokens.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tokenHash))
            {
                query = query.Where(t => t.TokenHash == tokenHash);
            }

            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId.Value);
            }

            if (tokenType is not null)
            {
                query = query.Where(t => t.TokenType == tokenType);
            }

            if (isUsed.HasValue)
            {
                query = query.Where(t => t.IsUsed == isUsed.Value);
            }

            if (isRevoked.HasValue)
            {
                query = query.Where(t => t.IsRevoked == isRevoked.Value);
            }

            if (!includeExpired)
            {
                var now = DateTime.UtcNow;
                query = query.Where(t => t.CreatedAt.Add(t.TokenType.Expiration) >= now);
            }

            return query;
        }

        public async Task AddAsync(UserToken token, CancellationToken cancellationToken = default)
        {
            await _dbContext.UserTokens.AddAsync(token, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserToken token, CancellationToken cancellationToken = default)
        {
            _dbContext.UserTokens.Update(token);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
