using MediatR;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.SetBlockStatus;

public class SetBlockStatusCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository)
        : IRequestHandler<SetBlockStatusCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(SetBlockStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.TargetUserId, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var blockResult = user.SetBlockStatus(
                isBlocked: request.IsBlocked,
                executedByUserId: _currentUser.Id.Value);

            if (blockResult.IsFailure)
            {
                return blockResult.Errors!;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return Done.Updated;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "SetBlockStatusCommandHandler",
                description: ex.Message
            );
        }
    }
}
