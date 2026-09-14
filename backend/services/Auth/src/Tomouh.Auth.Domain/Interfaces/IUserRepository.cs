using Tomouh.Auth.Domain.Entities;
using Tomouh.Shared.Kernel.Models;

namespace Tomouh.Auth.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByProviderSubjectIdAsync(string provider, object subjectId, CancellationToken cancellationToken);
    /// <summary>
    /// Retrieves a paginated list of users filtered by search criteria, email, IDs, and joining date range.
    /// </summary>
    /// <param name="searchTerm">A search term to filter by name or email (partial match).</param>
    /// <param name="email">Exact email address to filter by.</param>
    /// <param name="ids">Specific set of user identifiers to retrieve.</param>
    /// <param name="joinedAfter">Filter users who joined after this UTC date.</param>
    /// <param name="joinedBefore">Filter users who joined before this UTC date.</param>
    /// <param name="pageIndex">Zero-based index of the requested page (default is 0).</param>
    /// <param name="pageSize">Number of records per page (default is 12).</param>
    /// <param name="track">Flag to specify whether returned entities should be tracked in memory (default is true).</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A tuple containing the list of matching <see cref="User"/> instances and the total count before pagination.</returns>
    Task<PagedResult<User>> GetPagedAsync(
            string? searchTerm = null,
            string? email = null,
            IEnumerable<Guid>? ids = null,
            DateTime? joinedAfter = null,
            DateTime? joinedBefore = null,
            int pageIndex = 0,
            int pageSize = 12,
            bool track = true,
            CancellationToken cancellationToken = default);
}