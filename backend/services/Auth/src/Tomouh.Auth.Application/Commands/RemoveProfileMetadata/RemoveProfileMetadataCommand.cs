using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.RemoveProfileMetadata;

[Authorize]
public record RemoveProfileMetadataCommand(
    Role Role,
    string Key,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;
