using MediatR;
using Tomouh.Auth.Domain;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository,
    IPasswordHasher _passwordHasher)
        : IRequestHandler<ChangePasswordCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.Id.Value, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var verifyResult = user.IsCorrectPasswordHash(request.CurrentPassword, _passwordHasher);
            if (verifyResult.IsFailure || !verifyResult.Value)
            {
                return UserErrors.InvalidPassword;
            }

            var setPasswordResult = user.SetNewPassword(request.NewPassword, _passwordHasher);
            if (setPasswordResult.IsFailure)
            {
                return setPasswordResult.Errors!;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return Done.Updated;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "ChangePasswordCommandHandler",
                description: ex.Message
            );
        }
    }
}
