using MediatR;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.RemoveProfileMetadata;

public class RemoveProfileMetadataCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository)
        : IRequestHandler<RemoveProfileMetadataCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(RemoveProfileMetadataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.Id.Value, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var removeResult = user.RemoveProfileMetadata(
                role: request.Role,
                key: request.Key,
                executedByUserId: _currentUser.Id.Value);

            if (removeResult.IsFailure)
            {
                return removeResult.Errors!;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return Done.Updated;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "RemoveProfileMetadataCommandHandler",
                description: ex.Message
            );
        }
    }
}
