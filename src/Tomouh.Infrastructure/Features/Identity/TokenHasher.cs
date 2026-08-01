using Common.ResultOf;
using Common.ResultOf.Errors;
using Common.Services;
using System.Security.Cryptography;
using System.Text;

namespace Tomouh.Infrastructure.Features.Identity;

public class TokenHasher : ITokenHasher
{
    public ResultOf<string> Hash(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Error.Validation(description: "Value to hash cannot be null or empty.");

        try
        {
            var inputBytes = Encoding.UTF8.GetBytes(value);
            var hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes);
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
            var computedHashResult = Hash(value);
            if (computedHashResult.IsFailure)
                return computedHashResult.Errors;

            var isMatched = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHashResult.Value),
                Encoding.UTF8.GetBytes(hash));

            return isMatched;
        }
        catch (Exception ex)
        {
            return Error.Unexpected(description: $"An error occurred while verifying: {ex.Message}");
        }
    }
}