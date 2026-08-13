namespace Tomouh.Auth.Application.Common;

public record ExternalAuthPayload(
    string SubjectId,
    string Email,
    string FirstName,
    string LastName,
    string? PictureUrl = null,
    string? Name = null);