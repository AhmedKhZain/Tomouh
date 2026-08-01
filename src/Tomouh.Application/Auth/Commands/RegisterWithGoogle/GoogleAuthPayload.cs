namespace Tomouh.Application.Auth.Commands.RegisterWithGoogle;

public record GoogleAuthPayload(
    string SubjectId,
    string Email,
    string FirstName,
    string LastName,
    string Name,
    string? PictureUrl);