using Common.ResultOf;
using Common.Services;

namespace Common.Infrastructure.Features.Identity;

using Common.Errors;
using Common.Infrastructure.OptionsModels;
using Microsoft.Extensions.Options;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordOptions _options;

    public PasswordHasher(IOptions<PasswordOptions> options)
    {
        _options = options.Value;
    }

    public ResultOf<string> HashPassword(string password)
    {
        if (password.Length < _options.MinimumLength || password.Length > _options.MaximumLength)
            return Error.Validation(description: $"Password must be between {_options.MinimumLength} and {_options.MaximumLength} characters.");

        if (_options.EnforceUppercase && !password.Any(char.IsUpper))
            return Error.Validation(description: "Password must contain at least one uppercase letter.");

        if (_options.EnforceLowercase && !password.Any(char.IsLower))
            return Error.Validation(description: "Password must contain at least one lowercase letter.");

        if (_options.EnforceDigit && !password.Any(char.IsDigit))
            return Error.Validation(description: "Password must contain at least one digit.");

        if (_options.EnforceDelimiter && !password.Any(ch => !char.IsLetterOrDigit(ch)))
            return Error.Validation(description: "Password must contain at least one special character.");

        try
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password);

        }
        catch (Exception ex)
        {
            return Error.Unexpected(description: $"An error occurred while hashing the password: {ex.Message}");
        }

    }


    public ResultOf<bool> IsCorrectPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
        }
        catch (Exception ex)
        {
            return Error.Unexpected(description: $"An error occurred while verifying the password: {ex.Message}");
        }
    }
}