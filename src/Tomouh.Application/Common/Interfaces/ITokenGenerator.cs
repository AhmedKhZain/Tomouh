using Tomouh.Domain.Auth;

namespace Tomouh.Application.Common.Interfaces;

public interface ITokenGenerator
{
    string GenerateUserJwtToken(User user);
}
