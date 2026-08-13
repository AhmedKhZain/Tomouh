using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.AddOrUpdateProfileMetadata;

[Authorize]
public record AddOrUpdateProfileMetadataCommand(
    Role Role,
    string Key,
    string Value,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;
