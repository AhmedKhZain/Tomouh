using MediatR;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.UpdateUserData;

public class UpdateUserDataCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository)
        : IRequestHandler<UpdateUserDataCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(UpdateUserDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.Id.Value, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var updateResult = user.UpdateUserData(
                showName: request.ShowName,
                firstName: request.FirstName,
                lastName: request.LastName,
                email: request.Email);

            if (updateResult.IsFailure)
            {
                return updateResult.Errors!;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return Done.Updated;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "UpdateUserDataCommandHandler",
                description: ex.Message
            );
        }
    }
}
