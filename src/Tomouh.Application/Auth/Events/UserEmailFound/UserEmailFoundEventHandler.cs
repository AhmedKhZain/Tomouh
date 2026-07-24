using Common.Services;
using MediatR;
using Tomouh.Domain.Auth;
using Tomouh.Domain.Auth.Events;
using static Tomouh.Application.Auth.AuthenticationCommon;

namespace Tomouh.Application.Auth.Events.UserEmailFound
{
    internal class UserEmailFoundEventHandler(
        ICacheService<User> _cacheService
        ) : INotificationHandler<UserEmailFoundEvent>
    {
        public async Task Handle(UserEmailFoundEvent notification, CancellationToken cancellationToken)
        {
            await _cacheService.SetAsync(
                    cacheKey: $"{UserOptimisticLoadingCachePrefix}{notification.User.Email}",
                    notification.User,
                    expiration: UserOptimisticLoadingCacheExpiration
                );
        }
    }
}
