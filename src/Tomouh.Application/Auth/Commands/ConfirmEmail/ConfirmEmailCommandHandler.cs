using Common.Extinctions;
using Common.Models;
using Common.ResultOf;
using Common.ResultOf.Errors;
using Common.Services;
using MediatR;
using Tomouh.Application.Auth.Common;
using Tomouh.Domain.Auth;
using Tomouh.Domain.Auth.Repositories;

namespace Tomouh.Application.Auth.Queries.ConfirmEmail;

public class ConfirmEmailCommandHandler(
    IUserRepository _userRepository,
    IUserTokenRepository _tokenRepository,
    ITokenHasher _tokenHasher,
    CurrentUser _currentUser)
        : IRequestHandler<ConfirmEmailCommand, ResultOf<AuthenticationResult>>
{
    public async Task<ResultOf<AuthenticationResult>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.Id.Value, cancellationToken);

            var tokenEntity = await _tokenRepository.GetAsync(
                userId: user.Id,
                tokenType: TokenType.EmailConfirmation,
                cancellationToken: cancellationToken);

            user.ConfirmEmail(tokenEntity, request.Token, _tokenHasher);

            if (tokenEntity is null)
            {
                return AuthenticationCommon.AuthenticationErrors.EmailConfirmationTokenNotFound;
            }

            var confirmResult = user.ConfirmEmail(tokenEntity, request.Token, _tokenHasher);

            if (confirmResult.IsFailure)
                return confirmResult.Errors;

            await _userRepository.UpdateAsync(user, cancellationToken);

            return new AuthenticationResult(user, "Email confirmed successfully.").AsDone();
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "ConfirmEmailCommandHandler",
                description: ex.Message
            );
        }
    }
}
