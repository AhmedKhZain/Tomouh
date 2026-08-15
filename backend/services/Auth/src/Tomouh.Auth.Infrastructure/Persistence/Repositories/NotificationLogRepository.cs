using MongoDB.Driver;
using Tomouh.Auth.Infrastructure.Persistence.Contexts;
using Tomouh.Shared.Kernel.Features;
using Tomouh.Shared.Kernel.Outbox;

namespace Tomouh.Auth.Infrastructure.Persistence.Repositories;

public class NotificationLogRepository : INotificationLogRepository
{
    private readonly AuthContext _context;

    public NotificationLogRepository(AuthContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventOutbox>> GetLogs(int take = 20, CancellationToken cancellationToken = default)
    {
        var filter = Builders<EventOutbox>.Filter.Empty;
        var sort = Builders<EventOutbox>.Sort.Descending(x => x.CreatedAt);

        return await _context.FindListAsync(_context.NotificationLogs, filter, limit: take, sort: sort, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<EventOutbox>> GetLogsByUserIdAsync(Guid userId, int take = 20, CancellationToken cancellationToken = default)
    {
        var filter = Builders<EventOutbox>.Filter.Eq(x => x.CreatedBy, userId);
        var sort = Builders<EventOutbox>.Sort.Descending(x => x.CreatedAt);

        return await _context.FindListAsync(_context.NotificationLogs, filter, limit: take, sort: sort, cancellationToken: cancellationToken);
    }

    public async Task InsertAsync(EventOutbox eventData, CancellationToken cancellationToken = default)
    {
        await _context.InsertOneAsync(_context.NotificationLogs, eventData, cancellationToken);
    }

    public async Task InsertAsync(IEnumerable<EventOutbox> eventDatas, CancellationToken cancellationToken = default)
    {
        if (eventDatas is null || !eventDatas.Any())
        {
            return;
        }

        await _context.InsertManyAsync(_context.NotificationLogs, eventDatas, cancellationToken);
    }
}