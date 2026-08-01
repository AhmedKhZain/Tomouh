using System.Security.Claims;

namespace Common.Models;

public record CurrentUser(
    Guid? Id,
    string? FullName,
    string? Email,
    UserIP? UserIP,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<Claim> Claims
)
{
    public override string ToString()
    {
        return $"ID: {Id}, Name: {FullName}, Email: {Email}," +
            $"\nRoles: [{string.Join(", ", Roles)}]" +
            $"\nPermissions: [{string.Join(", ", Permissions)}]" +
            $"\nFrom IP: {UserIP}" +
            $"\nClaims: [{string.Join(" \n", Claims.Select(c => $"{c.Type}: {c.Value}"))}]";
    }

    public bool IsAuthenticated => Id.HasValue;

    // --- Role Helpers ---
    public bool IsInRole(string role) =>
        Roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<string> GetRoles() => Roles;

    // --- Permission Helpers ---
    public bool HasPermission(string permission) =>
        Permissions.Any(p => string.Equals(p, permission, StringComparison.OrdinalIgnoreCase));

    public bool HasAllPermissions(params string[] permissions) =>
        permissions.All(HasPermission);

    public bool HasAnyPermission(params string[] permissions) =>
        permissions.Any(HasPermission);

    public IReadOnlyList<string> GetPermissions() => Permissions;

    // --- Generic Claim Helpers ---
    public IReadOnlyList<string> GetClaimValues(string? claimType = null) =>
        Claims.Where(c => claimType == null || c.Type == claimType)
              .Select(c => c.Value)
              .ToArray();

    public IReadOnlyList<Claim> GetClaims(string? claimType = null) =>
        Claims.Where(c => claimType == null || c.Type == claimType)
              .ToArray();
}