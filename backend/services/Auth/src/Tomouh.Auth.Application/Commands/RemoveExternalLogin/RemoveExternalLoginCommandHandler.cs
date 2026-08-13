using MediatR;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.RemoveExternalLogin;

public sealed class RemoveExternalLoginCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository)
    : IRequestHandler<RemoveExternalLoginCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(
        RemoveExternalLoginCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.Id.Value, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var providerName = request.Provider.ToString();

            if (user.CanRemoveLastLogin)
            {
                return AuthenticationErrors.CannotRemoveLastAuthenticationMethod;
            }

            var removeResult = user.RemoveExternalLogin(providerName);
            if (removeResult.IsFailure)
            {
                return removeResult.Errors!;
            }

            return Done.Default;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "RemoveExternalLoginCommandHandler",
                description: ex.Message
            );
        }
    }
}