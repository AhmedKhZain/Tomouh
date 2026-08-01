using Common.ResultOf;

namespace Common.Services;

public interface ITokenHasher
{
    ResultOf<string> Hash(string value);
    ResultOf<bool> Verify(string value, string hash);

}
