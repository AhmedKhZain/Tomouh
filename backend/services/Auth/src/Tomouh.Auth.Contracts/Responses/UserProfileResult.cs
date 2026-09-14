using Tomouh.Auth.Domain.ValueObjects;

namespace Tomouh.Auth.Contracts.Responses;

public class UserProfileResult
{
    public string ProfileRole { get; init; } = string.Empty;
    public IReadOnlyCollection<AccountMetadata> MetaData { get; init; } = [];
}
