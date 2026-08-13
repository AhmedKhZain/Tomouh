using Tomouh.Auth.Application.Commands.AddOrUpdateProfileMetadata;
using Tomouh.Auth.Domain.Enums;

namespace Tomouh.Auth.Contracts;

public class AddOrUpdateProfileMetadataRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public AddOrUpdateProfileMetadataCommand ToCommand(Role role, Guid idempotencyKey)
        => new(role, Key, Value, idempotencyKey);
}
