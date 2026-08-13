using MediatR;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.GrantPermissionToProfile;

public class GrantPermissionToProfileCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository)
        : IRequestHandler<GrantPermissionToProfileCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(GrantPermissionToProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.TargetUserId, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var grantResult = user.GrantPermissionToProfile(
                role: request.Role,
                permission: request.Permission,
                executedByUserId: _currentUser.Id.Value);

            if (grantResult.IsFailure)
            {
                return grantResult.Errors!;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return Done.Updated;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "GrantPermissionToProfileCommandHandler",
                description: ex.Message
            );
        }
    }
}
