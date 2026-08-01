using Tomouh.Domain.Auth;

namespace Tomouh.Application.Common.Interfaces.Services;

public interface IJwtGenerator
{
    string GenerateUserJwtToken(User user);
}
