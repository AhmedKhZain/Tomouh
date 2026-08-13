using MediatR;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.RevokePermissionFromProfile;

public class RevokePermissionFromProfileCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository)
        : IRequestHandler<RevokePermissionFromProfileCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(RevokePermissionFromProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.TargetUserId, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var revokeResult = user.RevokePermissionFromProfile(
                role: request.Role,
                permission: request.Permission,
                executedByUserId: _currentUser.Id.Value);

            if (revokeResult.IsFailure)
            {
                return revokeResult.Errors!;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return Done.Updated;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "RevokePermissionFromProfileCommandHandler",
                description: ex.Message
            );
        }
    }
}
