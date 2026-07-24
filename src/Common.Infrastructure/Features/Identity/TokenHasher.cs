using Common.Errors;
using Common.ResultOf;
using Common.Services;

namespace Common.Infrastructure.Features.Identity;

public class TokenHasher : ITokenHasher
{
    public ResultOf<string> Hash(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Error.Validation(description: "Value to hash cannot be null or empty.");

        try
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(value);
        }
        catch (Exception ex)
        {
            return Error.Unexpected(description: $"An error occurred while hashing: {ex.Message}");
        }
    }

    public ResultOf<bool> Verify(string value, string hash)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(hash))
            return Error.Validation(description: "Value and hash cannot be null or empty.");

        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(value, hash);
        }
        catch (Exception ex)
        {
            return Error.Unexpected(description: $"An error occurred while verifying: {ex.Message}");
        }
    }
}
