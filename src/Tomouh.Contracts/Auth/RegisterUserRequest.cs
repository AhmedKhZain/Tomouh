using Tomouh.Application.Auth.Commands.Register;

namespace Tomouh.Contracts.Auth;

public class RegisterUserRequest
{
    public string ShowName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public RegisterUserCommand ToCommand(Guid requestId)
    {
        return new RegisterUserCommand(ShowName, FirstName, LastName, Email, Password, requestId);
    }
}
