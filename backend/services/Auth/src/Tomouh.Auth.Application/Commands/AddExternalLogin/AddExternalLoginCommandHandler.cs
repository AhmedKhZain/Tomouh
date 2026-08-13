using Google.Apis.Auth;
using MediatR;
using Tomouh.Auth.Application.Interfaces;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.AddExternalLogin;

public sealed class AddExternalLoginCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository,
    IExternalAuthProviderFactory _providerFactory)
    : IRequestHandler<AddExternalLoginCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(
        AddExternalLoginCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.Id.Value, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var strategy = _providerFactory.GetStrategy(request.Provider);
            var payload = await strategy.ValidateTokenAsync(request.Token, cancellationToken);

            var providerName = request.Provider.ToString();

            // Ensure this provider account is not already linked to another user
            var existingUserWithLogin = await _userRepository.GetByProviderSubjectIdAsync(
                providerName,
                payload.SubjectId,
                cancellationToken);

            if (existingUserWithLogin is not null)
            {
                return AuthenticationErrors.ExternalLoginAlreadyLinkedToAnotherUser;
            }

            var addResult = user.AddExternalLogin(providerName, payload.SubjectId);
            if (addResult.IsFailure)
            {
                return addResult.Errors!;
            }

            return Done.Created;
        }
        catch (InvalidJwtException)
        {
            return AuthenticationErrors.InvalidExternalToken;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "AddExternalLoginCommandHandler",
                description: ex.Message
            );
        }
    }
}