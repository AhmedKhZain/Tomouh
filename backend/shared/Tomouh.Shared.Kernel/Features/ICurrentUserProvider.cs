using Tomouh.Shared.Kernel.Models;

namespace Tomouh.Shared.Kernel.Features;

public interface ICurrentUserProvider
{
    CurrentUser? GetCurrentUser();

}
