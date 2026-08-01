using Common.ResultOf;
using Common.ResultOf.Errors;
using Common.Services;
using MediatR;
using Tomouh.Domain.Auth;
using Tomouh.Domain.Auth.Repositories;

namespace Tomouh.Application.Auth.Queries.ForgotPassword;

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
