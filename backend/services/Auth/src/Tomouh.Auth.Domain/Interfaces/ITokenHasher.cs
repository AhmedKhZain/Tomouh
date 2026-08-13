using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Domain.Interfaces;

public interface ITokenHasher
{
    ResultOf<string> Hash(string value);
    ResultOf<bool> Verify(string value, string hash);

}
