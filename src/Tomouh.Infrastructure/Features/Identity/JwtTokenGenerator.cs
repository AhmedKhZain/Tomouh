using Common.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tomouh.Application.Common.Interfaces.Services;
using Tomouh.Domain.Auth;
using Tomouh.Infrastructure.OptionsModels;

namespace Tomouh.Infrastructure.Features.Identity;

public class JwtGenerator : IJwtGenerator
{
    private readonly JwtOptions _jwtOptions;

    public JwtGenerator(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string GenerateUserJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(CustomClaimTypes.UserId, user.Id.ToString()),

            new(ClaimTypes.Name, user.FullName),
            new(CustomClaimTypes.Name, user.FullName),

            new(ClaimTypes.Email, user.Email.Email),
            new(CustomClaimTypes.Email, user.Email.Email)
        };

        var roles = user.Profiles
            .Select(p => p.Role.Name)
            .Distinct();

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var permissions = user.Profiles
            .SelectMany(p => p.Permissions)
            .Distinct();

        foreach (var permission in permissions)
        {
            claims.Add(new Claim(CustomClaimTypes.Permission, permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.TokenExpirationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}