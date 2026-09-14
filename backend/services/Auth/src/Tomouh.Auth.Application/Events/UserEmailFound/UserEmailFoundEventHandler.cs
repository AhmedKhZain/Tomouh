using MediatR;
using Tomouh.Auth.Domain.Events;
using Tomouh.Shared.Kernel.Features;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Events.UserEmailFound;

public class UserEmailFoundEventHandler(
    //IUserInterestRepository _userInterestRepository,
    ICacheService _cacheService
    ) : INotificationHandler<UserEmailFoundEvent>
{
    public async Task Handle(UserEmailFoundEvent notification, CancellationToken cancellationToken)
    {
        await _cacheService.SetAsync(
                cacheKey: $"{UserOptimisticLoadingCachePrefix}{notification.User.MainEmail.Email}",
                notification.User,
                expiration: UserOptimisticLoadingCacheExpiration
            );

        //var userInterests = await _userInterestRepository.GetAll(
        //    userId: notification.User.Id,
        //    page: 0,
        //    pageSize: 24);

        //if (userInterests.Any())
        //{
        //    await _cacheService.SetAsync(
        //        cacheKey: $"UserInterests:userId:{notification.User.Id}",
        //        userInterests,
        //        UserOptimisticLoadingCacheExpiration);
        //}

    }
}
