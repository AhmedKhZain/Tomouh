using Tomouh.Application.Common.Models;

namespace Tomouh.Application.Common.Interfaces.Repositories;

public interface INotificationLogRepository
{
    Task InsertAsync(EventOutbox eventData, CancellationToken cancellationToken = default);
    Task InsertAsync(IEnumerable<EventOutbox> eventDatas, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventOutbox>> GetLogsByUserIdAsync(Guid userId, int take = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventOutbox>> GetLogs(int take = 20, CancellationToken cancellationToken = default);
}
