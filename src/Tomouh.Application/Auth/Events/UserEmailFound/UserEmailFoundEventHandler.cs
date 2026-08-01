using Common.Services;
using MediatR;
using Tomouh.Domain.Auth.Events;
using static Tomouh.Application.Auth.Common.AuthenticationCommon;

namespace Tomouh.Application.Auth.Events.UserEmailFound;

public class UserEmailFoundEventHandler(
    //IUserInterestRepository _userInterestRepository,
    ICacheService _cacheService
    ) : INotificationHandler<UserEmailFoundEvent>
{
    public async Task Handle(UserEmailFoundEvent notification, CancellationToken cancellationToken)
    {
        await _cacheService.SetAsync(
                cacheKey: $"{UserOptimisticLoadingCachePrefix}{notification.User.Email}",
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
