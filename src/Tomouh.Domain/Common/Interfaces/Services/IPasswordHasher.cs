using Common.ResultOf;

namespace Common.Services;

public interface IPasswordHasher
{

    public ResultOf<string> HashPassword(string password);
    public ResultOf<bool> IsCorrectPassword(string password, string hash);

}
