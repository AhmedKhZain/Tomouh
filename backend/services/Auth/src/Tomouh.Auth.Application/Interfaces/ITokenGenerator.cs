using Tomouh.Auth.Domain.Entities;

namespace Tomouh.Auth.Application.Interfaces;

public interface IJwtGenerator
{
    string GenerateUserJwt(User user);
}
