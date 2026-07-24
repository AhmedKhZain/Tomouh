using MongoDB.Driver;
using Tomouh.Domain.Auth;
using Tomouh.Domain.Auth.Repositories;

namespace Tomouh.Infrastructure.Persistence.NoSql.Repositories;

/// <summary>
/// Repository implementation for managing <see cref="User"/> aggregate persistence operations using MongoDB.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly TomouhMongoContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The MongoDB context and entity tracker instance.</param>
    public UserRepository(TomouhMongoContext context)
    {
        _context = context;
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

        // Fetch from MongoDB if not found in memory
        var user = await _context.Users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

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
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.TrackAggregate(user);

        await _context.Users.InsertOneAsync(user, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Replaces an existing user document in MongoDB with the updated entity state and registers its events.
    /// </summary>
    /// <param name="user">The user entity containing updated state.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.TrackAggregate(user);

        await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves a user by their email address, searching in-memory tracked entities first before hitting the database.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The matching <see cref="User"/> instance if found; otherwise, <c>null</c>.</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Check in-memory tracked entities first
        var trackedUser = _context.GetTrackedEntities<User>()
            .FirstOrDefault(u => u.Email.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (trackedUser is not null)
        {
            return trackedUser;
        }

        // Fetch from MongoDB if not found in memory
        var user = await _context.Users
            .Find(u => u.Email.Email == email)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is not null)
        {
            _context.TrackAggregate(user);
            return user;
        }

        return null;
    }
}