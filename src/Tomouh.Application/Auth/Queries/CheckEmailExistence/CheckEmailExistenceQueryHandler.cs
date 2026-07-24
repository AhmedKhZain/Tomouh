using Common.Errors;
using Common.Extinctions;
using Common.ResultOf;
using MediatR;
using Tomouh.Application.Auth.Common;
using Tomouh.Domain.Auth.Repositories;
using static Tomouh.Application.Auth.AuthenticationCommon;

namespace Tomouh.Application.Auth.Queries.CheckEmailExistence;

public class CheckEmailExistenceQueryHandler(
    IUserRepository _userRepository
    ) : IRequestHandler<CheckEmailExistenceQuery, ResultOf<AuthenticationResultBase>>
{
    public async Task<ResultOf<AuthenticationResultBase>> Handle(CheckEmailExistenceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                return AuthenticationErrors.UserNotFound(request.Email);

            if (user.Status.IsBlocked)
                return AuthenticationErrors.UserBlocked(request.Email);

            if (!user.Status.IsActive)
                return AuthenticationErrors.UserNotActive(request.Email);

            user.MarkEmailFound();


            return new AuthenticationResultBase(user).AsDone();
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "CheckEmailExistenceQueryHandler",
                description: $"{ex.Message}");
        }
    }
}