namespace Tomouh.Domain.UserInterests.Repositories;

public interface IUserInterestRepository
{
    Task<IEnumerable<UserInterest>> GetAll(Guid? userId = null, Guid? scholarshipId = null, int page = 0, int pageSize = 12);
    Task AddAsync(UserInterest userInterest);
}
