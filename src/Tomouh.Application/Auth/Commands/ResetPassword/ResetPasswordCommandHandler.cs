using Common.ResultOf;
using Common.ResultOf.Errors;
using Common.Services;
using MediatR;
using Tomouh.Application.Auth.Common;
using Tomouh.Domain.Auth;
using Tomouh.Domain.Auth.Repositories;

namespace Tomouh.Application.Auth.Queries.ResetPassword;

public class ResetPasswordCommandHandler(
    IUserRepository _userRepository,
    IUserTokenRepository _tokenRepository,
    IPasswordHasher _passwordHasher,
    ITokenHasher _tokenHasher)
        : IRequestHandler<ResetPasswordCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var hashResult = _tokenHasher.Hash(request.Token);
            if (hashResult.IsFailure)
                return hashResult.Errors;

            var tokenEntity = await _tokenRepository.GetAsync(
                tokenHash: hashResult.Value,
                tokenType: TokenType.PasswordReset,
                isUsed: false,
                includeExpired: false,
                cancellationToken: cancellationToken);

            if (tokenEntity is null)
            {
                return AuthenticationCommon.AuthenticationErrors.InvalidResetToken;
            }

            var user = await _userRepository.GetByIdAsync(tokenEntity.UserId, cancellationToken);
            if (user is null)
            {
                return AuthenticationCommon.AuthenticationErrors.UserNotFound;
            }

            var markUsedResult = tokenEntity.MarkUsed(request.Token, _tokenHasher);
            if (markUsedResult.IsFailure)
                return markUsedResult.Errors;

            var setPasswordResult = user.SetNewPassword(request.NewPassword, _passwordHasher);
            if (setPasswordResult.IsFailure)
                return setPasswordResult.Errors;

            await _tokenRepository.UpdateAsync(tokenEntity, cancellationToken);
            await _userRepository.UpdateAsync(user, cancellationToken);

            return Done.Updated;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "ResetPasswordCommandHandler",
                description: ex.Message
            );
        }
    }
}
