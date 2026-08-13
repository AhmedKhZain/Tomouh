using MongoDB.Driver;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Auth.Infrastructure.Persistence.Contexts;
using Tomouh.Shared.Kernel.Models;

namespace Tomouh.Auth.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for managing <see cref="User"/> aggregate persistence operations using MongoDB.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AuthContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The MongoDB context and entity tracker instance.</param>
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

        // إرجاع PagedResult أنظف بكتير من الـ Tuple
        return new PagedResult<User>(items, totalCount);
    }



    /// <summary>
    /// Retrieves a user by their unique identifier, prioritizing memory-tracked instances before querying the database.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The matching <see cref="User"/> instance if found; otherwise, <c>null</c>.</returns>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Check in-memory tracked entities first
        var trackedUser = _context.GetTrackedEntity<User, Guid>(id);
        if (trackedUser is not null)
        {
            return trackedUser;
        }

        // Fetch from MongoDB adhering to active transaction session if available
        var user = await _context.FirstOrDefaultAsync(
            _context.Users,
            u => u.Id == id,
            cancellationToken);

        if (user is not null)
        {
            _context.TrackAggregate(user);
            return user;
        }

        return null;
    }

    /// <summary>
    /// Retrieves a user by their email address, searching in-memory tracked entities first before hitting the database.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The matching <see cref="User"/> instance if found; otherwise, <c>null</c>.</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Check in-memory tracked entities first with safe null navigation
        var trackedUser = _context.GetTrackedEntities<User>()
            .FirstOrDefault(u => u.MainEmail != null && u.MainEmail.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (trackedUser is not null)
        {
            return trackedUser;
        }

        // Fetch from MongoDB adhering to active transaction session if available
        var user = await _context.FirstOrDefaultAsync(
            _context.Users,
            u => u.MainEmail.Email == email,
            cancellationToken);

        if (user is not null)
        {
            _context.TrackAggregate(user);
            return user;
        }

        return null;
    }

    /// <summary>
    /// Retrieves a user by an external OAuth provider and their unique subject identifier (e.g., Google, Facebook).
    /// </summary>
    /// <param name="provider">The identity provider name (e.g., "Google").</param>
    /// <param name="subjectId">The provider specific subject identifier.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The matching <see cref="User"/> instance if found; otherwise, <c>null</c>.</returns>
    public async Task<User?> GetByProviderSubjectIdAsync(string provider, object subjectId, CancellationToken cancellationToken = default)
    {
        var subjectIdStr = subjectId?.ToString() ?? string.Empty;

        // Check in-memory tracked entities first with safe null navigation
        var trackedUser = _context.GetTrackedEntities<User>()
            .FirstOrDefault(u => u.ExternalLogins != null &&
                                 u.ExternalLogins.Any(l => l.Provider == provider && l.SubjectId == subjectIdStr));

        if (trackedUser is not null)
        {
            return trackedUser;
        }

        // Fetch from MongoDB adhering to active transaction session if available
        var user = await _context.FirstOrDefaultAsync(
            _context.Users,
            u => u.ExternalLogins != null && u.ExternalLogins.Any(l => l.Provider == provider && l.SubjectId == subjectIdStr),
            cancellationToken);

        if (user is not null)
        {
            _context.TrackAggregate(user);
            return user;
        }

        return null;
    }

    /// <summary>
    /// Adds a new user entity to the MongoDB collection and tracks its domain state.
    /// </summary>
    /// <param name="user">The user entity to insert.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.TrackAggregate(user);

        await _context.InsertOneAsync(_context.Users, user, cancellationToken);
    }

    /// <summary>
    /// Replaces an existing user document in MongoDB with the updated entity state and registers its events.
    /// </summary>
    /// <param name="user">The user entity containing updated state.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.TrackAggregate(user, replaceExisting: true);

        var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);

        await _context.ReplaceOneAsync(_context.Users, filter, user, cancellationToken);
    }
}