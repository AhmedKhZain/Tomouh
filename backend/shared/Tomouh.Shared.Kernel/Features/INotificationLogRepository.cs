using Tomouh.Shared.Kernel.Outbox;

namespace Tomouh.Shared.Kernel.Features;

public interface INotificationLogRepository
{
    Task InsertAsync(EventOutbox eventData, CancellationToken cancellationToken = default);
    Task InsertAsync(IEnumerable<EventOutbox> eventDatas, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventOutbox>> GetLogsByUserIdAsync(Guid userId, int take = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventOutbox>> GetLogs(int take = 20, CancellationToken cancellationToken = default);
}
