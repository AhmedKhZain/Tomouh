using MediatR;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.AddOrUpdateProfileMetadata;

public class AddOrUpdateProfileMetadataCommandHandler(
    CurrentUser _currentUser,
    IUserRepository _userRepository)
        : IRequestHandler<AddOrUpdateProfileMetadataCommand, ResultOf<Done>>
{
    public async Task<ResultOf<Done>> Handle(AddOrUpdateProfileMetadataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.Id.Value, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            var metadataResult = user.AddOrUpdateProfileMetadata(
                role: request.Role,
                key: request.Key,
                value: request.Value,
                executedByUserId: _currentUser.Id.Value);

            if (metadataResult.IsFailure)
            {
                return metadataResult.Errors!;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return Done.Updated;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "AddOrUpdateProfileMetadataCommandHandler",
                description: ex.Message
            );
        }
    }
}
