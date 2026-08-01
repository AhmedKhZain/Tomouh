using Common.Models;
using Common.Services;
using System.Security.Claims;

namespace Tomouh.API.Services;

public class CurrentUserProvider(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public CurrentUser GetCurrentUser()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return Anonymous();
        }

        var user = httpContext.User;
        var userIP = ExtractUserIP(httpContext);

        if (user is null || user.Identity?.IsAuthenticated != true)
        {
            return Anonymous(userIP);
        }

        var userId = ReadGuidClaim(user, ClaimTypes.NameIdentifier)
                  ?? ReadGuidClaim(user, CustomClaimTypes.UserId);

        var fullName = user.FindFirst(ClaimTypes.Name)?.Value
                    ?? user.FindFirst(CustomClaimTypes.Name)?.Value;

        var email = user.FindFirst(ClaimTypes.Email)?.Value
                 ?? user.FindFirst(CustomClaimTypes.Email)?.Value;

        var roles = user.FindAll(ClaimTypes.Role)
                        .Select(c => c.Value)
                        .Distinct()
                        .ToList();

        var permissions = user.FindAll(CustomClaimTypes.Permission)
                              .Select(c => c.Value)
                              .Distinct()
                              .ToList();

        var claims = user.Claims.ToList();

        return new CurrentUser(
            Id: userId,
            FullName: fullName,
            Email: email,
            UserIP: userIP,
            Roles: roles,
            Permissions: permissions,
            Claims: claims
        );
    }

    private static CurrentUser Anonymous(UserIP? userIP = null) =>
        new(
            Id: null,
            FullName: null,
            Email: null,
            UserIP: userIP,
            Roles: Array.Empty<string>(),
            Permissions: Array.Empty<string>(),
            Claims: Array.Empty<Claim>()
        );

    private static UserIP? ExtractUserIP(HttpContext httpContext)
    {
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        string? ipString = !string.IsNullOrWhiteSpace(forwardedFor)
            ? forwardedFor.Split(',')[0].Trim()
            : httpContext.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrWhiteSpace(ipString))
        {
            return null;
        }

        try
        {
            return UserIP.FromString(ipString);
        }
        catch
        {
            return null;
        }
    }

    private static Guid? ReadGuidClaim(ClaimsPrincipal? principal, string claimType)
    {
        if (principal is null) return null;

        var value = principal.FindFirst(claimType)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}