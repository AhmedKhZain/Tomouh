using Common.ResultOf;
using Common.ResultOf.Errors;
using Common.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Tomouh.Infrastructure.OptionsModels;

namespace Tomouh.Infrastructure.Features.Identity;

public class PasswordHasher : IPasswordHasher
{
    private readonly LocalPasswordOptions _options;
    private readonly PasswordHasher<object> _hasher;
    private readonly object _dummyUser = new();

    public PasswordHasher(IOptions<LocalPasswordOptions> options)
    {
        _options = options.Value;
        _hasher = new PasswordHasher<object>();
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
            var hash = _hasher.HashPassword(_dummyUser, password);
            return hash;
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
            var result = _hasher.VerifyHashedPassword(_dummyUser, hash, password);

            return result != PasswordVerificationResult.Failed;
        }
        catch (Exception ex)
        {
            return Error.Unexpected(description: $"An error occurred while verifying the password: {ex.Message}");
        }
    }
}