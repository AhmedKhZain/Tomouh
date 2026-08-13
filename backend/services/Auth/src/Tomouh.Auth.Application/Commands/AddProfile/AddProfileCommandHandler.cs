using MediatR;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.AddProfile;

public class AddProfileCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository)
        : IRequestHandler<AddProfileCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(AddProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.Id.Value, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var addProfileResult = user.AddProfile(request.Role, _currentUser.Id.Value);
            if (addProfileResult.IsFailure)
            {
                return addProfileResult.Errors!;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return Done.Created;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "AddProfileCommandHandler",
                description: ex.Message
            );
        }
    }
}
