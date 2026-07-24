using System.Security.Claims;

namespace Common.Models;

public record CurrentUser(
    Guid? Id,
    string? FullName,
    string? Email,
    UserIP? UserIP,
    IReadOnlyList<string> Roles,
    IReadOnlyList<Claim> Claims
)
{
    public override string ToString()
    {
        return $"ID: {Id}, Name: {FullName}, Email: {Email}," +
            $"\nRole: {string.Join("     \n", Roles)}" +
            $"\nFrom IP: {UserIP}" +
            $"\nClaims: [{string.Join("     \n", Claims.Select(c => $"{c.Type}: {c.Value}"))}]";
    }
    public bool IsAuthenticated => Id.HasValue;

    public bool IsInRole(string role) =>
        Roles.Any(r => r == role);

    public IReadOnlyList<string> GetClaimValues(string claimType = null) =>
        Claims.Where(c => c.Type == claimType || claimType == null)
              .Select(c => c.Value)
              .ToArray();

    public IReadOnlyList<Claim> GetClaims(string claimType = null) =>
        Claims.Where(c => c.Type == claimType || claimType == null)
              .ToArray();
    public IReadOnlyList<string> GetRoles() =>
        Roles.ToArray();


}
