using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.ValueObjects;
using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.AddOrUpdateProfileMetadata;

[Authorize]
public record AddOrUpdateProfileMetadataCommand(
    Role Role,
    string Key,
    string Value,
    bool IsPublic,
    AccountMetadataType MetadataType,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;
