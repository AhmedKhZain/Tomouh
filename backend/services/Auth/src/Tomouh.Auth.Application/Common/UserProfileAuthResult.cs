namespace Tomouh.Auth.Application.Common;

public class UserProfileAuthResult
{
    public string ProfileRole { get; init; }
    public IReadOnlyDictionary<string, string> MetaData { get; init; }
}
