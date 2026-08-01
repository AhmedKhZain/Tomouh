using MongoDB.Driver.Linq;
using Tomouh.Application.Common.Interfaces.Repositories;
using Tomouh.Application.Common.Models;

namespace Tomouh.Infrastructure.Persistence.Sql.Repositories;

public class NotificationLogRepository : INotificationLogRepository
{
    private readonly AppSystemDbContext _context;

    public NotificationLogRepository(AppSystemDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventOutbox>> GetLogs(int take = 20, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxEvents.Take(take).ToListAsync();
    }

    public async Task<IEnumerable<EventOutbox>> GetLogsByUserIdAsync(Guid userId, int take = 20, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxEvents.Where(e => e.CreatedBy == userId).Take(take).ToListAsync();
    }

    public async Task InsertAsync(EventOutbox eventData, CancellationToken cancellationToken = default)
    {
        await _context.AddAsync(eventData, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task InsertAsync(IEnumerable<EventOutbox> eventDatas, CancellationToken cancellationToken = default)
    {
        await _context.AddRangeAsync(eventDatas, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
