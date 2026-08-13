using MediatR;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.SetAccountActivationStatus;

public class SetAccountActivationStatusCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository)
        : IRequestHandler<SetAccountActivationStatusCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(SetAccountActivationStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.TargetUserId, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var statusResult = user.SetAccountActivationStatus(
                isActive: request.IsActive,
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
                code: "SetAccountActivationStatusCommandHandler",
                description: ex.Message
            );
        }
    }
}
