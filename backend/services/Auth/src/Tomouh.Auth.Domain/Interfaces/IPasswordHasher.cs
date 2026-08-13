using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Domain.Interfaces;

public interface IPasswordHasher
{

    public ResultOf<string> HashPassword(string password);
    public ResultOf<bool> IsCorrectPassword(string password, string hash);

}
