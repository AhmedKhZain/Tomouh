using Tomouh.Auth.Application.Commands.UpdateUserData;

namespace Tomouh.Auth.Contracts;

public class UpdateUserDataRequest
{
    public string? ShowName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }

    public UpdateUserDataCommand ToCommand(Guid idempotencyKey)
        => new(ShowName, FirstName, LastName, Email, idempotencyKey);
}
