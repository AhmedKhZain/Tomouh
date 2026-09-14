using Tomouh.Auth.Domain.Enums;

namespace Tomouh.Auth.Contracts.Requests;

public class AddOrUpdateProfileMetadataRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}