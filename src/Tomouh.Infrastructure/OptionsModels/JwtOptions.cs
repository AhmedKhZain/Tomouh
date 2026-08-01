namespace Tomouh.Infrastructure.OptionsModels;


public class JwtOptions
{
    public string Key { get; set; } = null!;
    public int TokenExpirationInMinutes { get; set; }
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
}
