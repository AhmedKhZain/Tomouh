using MediatR;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.SetCommentingStatus;

public class SetCommentingStatusCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository)
        : IRequestHandler<SetCommentingStatusCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(SetCommentingStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.TargetUserId, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var statusResult = user.SetCommentingStatus(
                isDisabled: request.IsDisabled,
                executedByUserId: _currentUser.Id.Value);

            if (statusResult.IsFailure)
            {
                return statusResult.Errors!;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return Done.Updated;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "SetCommentingStatusCommandHandler",
                description: ex.Message
            );
        }
    }
}
