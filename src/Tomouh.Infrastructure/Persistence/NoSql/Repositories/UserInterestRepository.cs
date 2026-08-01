using MongoDB.Driver;
using Tomouh.Domain.UserInterests;
using Tomouh.Domain.UserInterests.Repositories;

namespace Tomouh.Infrastructure.Persistence.NoSql.Repositories;

internal class UserInterestRepository : IUserInterestRepository
{
    private readonly TomouhMongoContext _context;

    public UserInterestRepository(TomouhMongoContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserInterest userInterest)
    {
        await _context.UserInterests.InsertOneAsync(userInterest);
    }

    public async Task<IEnumerable<UserInterest>> GetAll(Guid? userId = null, Guid? scholarshipId = null, int page = 0, int pageSize = 12)
    {
        return await _context.UserInterests
            .Find(ui =>
            (userId == null || ui.UserId == userId)
            && (scholarshipId == null || ui.ScholarshipId == scholarshipId))
            .Skip(page * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }
}
