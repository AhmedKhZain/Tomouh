using MongoDB.Driver;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Auth.Infrastructure.Persistence.Contexts;
using Tomouh.Shared.Kernel.Models;

namespace Tomouh.Auth.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthContext _context;

    public UserRepository(AuthContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<User>> GetPagedAsync(
        string? searchTerm = null,
        string? email = null,
        IEnumerable<Guid>? ids = null,
        DateTime? joinedAfter = null,
        DateTime? joinedBefore = null,
        int pageIndex = 0,
        int pageSize = 12,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        var builder = Builders<User>.Filter;
        var filters = new List<FilterDefinition<User>>();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchFilter = builder.Or(
                builder.Regex(u => u.Name.FirstName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                builder.Regex(u => u.Name.LastName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                builder.Regex(u => u.MainEmail.Email, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );
            filters.Add(searchFilter);
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            filters.Add(builder.Eq(u => u.MainEmail.Email, email));
        }

        if (ids is not null && ids.Any())
        {
            filters.Add(builder.In(u => u.Id, ids));
        }

        if (joinedAfter.HasValue)
        {
            filters.Add(builder.Gte(u => u.CreatedAt, joinedAfter.Value));
        }

        if (joinedBefore.HasValue)
        {
            filters.Add(builder.Lte(u => u.CreatedAt, joinedBefore.Value));
        }

        var combinedFilter = filters.Count > 0
            ? builder.And(filters)
            : builder.Empty;

        var (items, totalCount) = await _context.GetPagedAsync(
            _context.Users,
            combinedFilter,
            pageIndex,
            pageSize,
            cancellationToken);

        if (track)
        {
            foreach (var user in items)
            {
                _context.TrackAggregate(user);
            }
        }

        return new PagedResult<User>(items, totalCount);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var trackedUser = _context.GetTrackedEntity(id);
        if (trackedUser is not null)
        {
            return trackedUser;
        }

        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        var user = await _context.FirstOrDefaultAsync(
            _context.Users,
            filter,
            cancellationToken);

        if (user is not null)
        {
            _context.TrackAggregate(user);
            return user;
        }

        return null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var trackedUser = _context.GetTrackedEntities()
            .FirstOrDefault(u => u.MainEmail != null && u.MainEmail.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (trackedUser is not null)
        {
            return trackedUser;
        }
        var Filter = Builders<User>.Filter.Eq(u => u.MainEmail.Email, email);

        var user = await _context.FirstOrDefaultAsync(
            _context.Users,
            Filter,
            cancellationToken);

        if (user is not null)
        {
            _context.TrackAggregate(user);
            return user;
        }

        return null;
    }

    public async Task<User?> GetByProviderSubjectIdAsync(string provider, object subjectId, CancellationToken cancellationToken = default)
    {
        var subjectIdStr = subjectId?.ToString() ?? string.Empty;

        var trackedUser = _context.GetTrackedEntities()
            .FirstOrDefault(u => u.ExternalLogins != null &&
                                 u.ExternalLogins.Any(l => l.Provider == provider && l.SubjectId == subjectIdStr));

        if (trackedUser is not null)
        {
            return trackedUser;
        }

        var filter = Builders<User>.Filter.ElemMatch(u => u.ExternalLogins, l => l.Provider == provider && l.SubjectId == subjectIdStr);

        var user = await _context.FirstOrDefaultAsync(
            _context.Users,
            filter,
            cancellationToken);

        if (user is not null)
        {
            _context.TrackAggregate(user);
            return user;
        }

        return null;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.TrackAggregate(user);

        await _context.InsertOneAsync(_context.Users, user, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.TrackAggregate(user, replaceExisting: true);

        var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);

        await _context.ReplaceOneAsync(_context.Users, filter, user, cancellationToken);
    }
}
