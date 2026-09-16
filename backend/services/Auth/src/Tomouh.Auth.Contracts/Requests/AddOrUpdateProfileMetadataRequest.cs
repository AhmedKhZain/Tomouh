using Tomouh.Auth.Domain.ValueObjects;

namespace Tomouh.Auth.Contracts.Requests;

public class AddOrUpdateProfileMetadataRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;
    public AccountMetadataType MetadataType { get; set; } = AccountMetadataType.String;
}