using MediatR;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.ChangeTFAStatus;

public class ChangeTFAStatusCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository,
    IPasswordHasher _passwordHasher)
        : IRequestHandler<ChangeTFAStatusCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(ChangeTFAStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.Id.Value, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var tfaResult = user.ChangeTFAStatus(
                isEnabled: request.IsEnabled,
                password: request.Password,
                passwordHasher: _passwordHasher);

            if (tfaResult.IsFailure)
            {
                return tfaResult.Errors!;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return Done.Updated;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "ChangeTFAStatusCommandHandler",
                description: ex.Message
            );
        }
    }
}
