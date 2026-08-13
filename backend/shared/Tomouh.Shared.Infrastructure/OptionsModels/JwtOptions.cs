namespace Tomouh.Shared.Infrastructure.OptionsModels;

public class JwtOptions
{
    public int TokenExpirationInMinutes { get; set; }
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
}