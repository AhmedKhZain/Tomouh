using Tomouh.Application.Auth.Commands.RegisterWithGoogle;
using Tomouh.Application.Auth.Queries.LoginWithGoogle;

namespace Tomouh.Contracts.Auth;

public class GoogleAuthRequest
{
    public string GoogleToken { get; set; }
    public LoginWithGoogleQuery ToLoginQuery(Guid requestId)
    {
        return new LoginWithGoogleQuery(GoogleToken, requestId);
    }
    public RegisterWithGoogleCommand ToRegisterCommand(Guid requestId)
    {
        return new RegisterWithGoogleCommand(GoogleToken, requestId);
    }
}
