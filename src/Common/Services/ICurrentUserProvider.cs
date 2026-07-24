using Common.Models;

namespace Common.Services;

public interface ICurrentUserProvider
{
    CurrentUser? GetCurrentUser();

}
