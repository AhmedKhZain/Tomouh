using MediatR;
using Tomouh.Auth.Application.Common;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;

namespace Tomouh.Auth.Application.Commands.ResetPassword;

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
