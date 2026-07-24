using Common.Enums;
using System.Security.Cryptography;
using System.Text;

namespace Tomouh.Domain.Auth;


public class TokenType : SmartEnum<TokenType>
{
    public TimeSpan Expiration { get; private set; }
    private static readonly TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(10);
    private static readonly TimeSpan EmailConfirmationExpiration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan PasswordResetExpiration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan TwoFactorAuthenticationExpiration = TimeSpan.FromMinutes(10);


    public static readonly TokenType RefreshToken = new TokenType("RefreshToken", 1, RefreshTokenExpiration);
    public static readonly TokenType EmailConfirmation = new TokenType("EmailConfirmation", 2, EmailConfirmationExpiration);
    public static readonly TokenType PasswordReset = new TokenType("PasswordReset", 3, PasswordResetExpiration);
    public static readonly TokenType TwoFactorAuthentication = new TokenType("TwoFactorAuthentication", 4, TwoFactorAuthenticationExpiration);
    private TokenType(string name, int value, TimeSpan expiration) : base(name, value)
    {
        Expiration = expiration;
    }

    internal string? Create()
    {
        string token;

        switch (this.Value)
        {
            case 4: // RefreshToken
                token = GenerateSecureRandomString(128);
                break;

            case 1: // EmailConfirmation
                token = GenerateSecureRandomString(6);
                break;
            case 2: // PasswordReset
                token = GenerateSecureRandomString(6);
                break;

            case 3: // TwoFactorAuthentication
                token = GenerateNumericCode(6);
                break;

            default:
                throw new ArgumentException("Unsupported token type");
        }


        return token;

    }


    private static string GenerateSecureRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var data = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(data);
        }

        var result = new StringBuilder(length);
        foreach (byte b in data)
        {
            result.Append(chars[b % chars.Length]);
        }

        return result.ToString();
    }

    private static string GenerateNumericCode(int length)
    {
        var rng = new Random();
        return rng.Next((int)Math.Pow(10, length - 1), (int)Math.Pow(10, length)).ToString();
    }
}
