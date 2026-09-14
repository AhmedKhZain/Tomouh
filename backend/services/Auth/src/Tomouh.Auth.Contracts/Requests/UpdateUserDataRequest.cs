namespace Tomouh.Auth.Contracts.Requests;

public class UpdateUserDataRequest
{
    public string? ShowName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}