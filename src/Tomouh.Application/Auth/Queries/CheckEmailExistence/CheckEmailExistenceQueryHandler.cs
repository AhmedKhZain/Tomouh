using Common.Extinctions;
using Common.ResultOf;
using Common.ResultOf.Errors;
using MediatR;
using Tomouh.Application.Auth.Common;
using Tomouh.Domain.Auth.Repositories;
using static Tomouh.Application.Auth.Common.AuthenticationCommon;

namespace Tomouh.Application.Auth.Queries.CheckEmailExistence;

public class CheckEmailExistenceQueryHandler(
    IUserRepository _userRepository
    ) : IRequestHandler<CheckEmailExistenceQuery, ResultOf<AuthenticationResult>>
{
    public async Task<ResultOf<AuthenticationResult>> Handle(CheckEmailExistenceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                return AuthenticationErrors.UserNotFound;

            if (user.Status.IsBlocked)
                return AuthenticationErrors.UserBlocked;

            if (!user.Status.IsActive)
                return AuthenticationErrors.UserIsDeactivated;

            user.MarkEmailFound();


            return new AuthenticationResult(user).AsDone();
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "CheckEmailExistenceQueryHandler",
                description: $"{ex.Message}");
        }
    }
}