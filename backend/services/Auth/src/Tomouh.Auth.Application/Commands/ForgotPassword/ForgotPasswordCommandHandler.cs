using MediatR;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;

namespace Tomouh.Auth.Application.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IUserRepository _userRepository,
    ITokenHasher _tokenHasher)
        : IRequestHandler<ForgotPasswordCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                return Done.Default;
            }
            var tokenCreationResult = user.GenerateToken(
                tokenType: TokenType.PasswordReset,
                hasher: _tokenHasher,
                out var token);
            if (tokenCreationResult.IsFailure)
                return tokenCreationResult.Errors;

            return Done.Default;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "ForgotPasswordCommandHandler",
                description: ex.Message
            );
        }
    }
}
