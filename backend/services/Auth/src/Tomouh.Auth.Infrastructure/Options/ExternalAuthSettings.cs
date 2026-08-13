namespace Tomouh.Auth.Infrastructure.Options;

public class ExternalAuthSettings
{
    public const string SectionName = "ExternalAuth";

    public ProviderSettings Google { get; init; } = new();
    public ProviderSettings GitHub { get; init; } = new();
    public MicrosoftProviderSettings Microsoft { get; init; } = new();
}

public class ProviderSettings
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
}

public class MicrosoftProviderSettings : ProviderSettings
{
    public string TenantId { get; init; } = "common";
}