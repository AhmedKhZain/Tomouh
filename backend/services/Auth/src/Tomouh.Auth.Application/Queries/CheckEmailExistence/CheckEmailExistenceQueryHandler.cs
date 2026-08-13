using MediatR;
using Tomouh.Auth.Application.Common;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Extensions;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Queries.CheckEmailExistence;

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