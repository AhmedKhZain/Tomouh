using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Tomouh.Auth.Application.Interfaces;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Shared.Infrastructure.OptionsModels;
using Tomouh.Shared.Kernel.Models;

namespace Tomouh.Auth.Infrastructure.Identity;

public class JwtGenerator : IJwtGenerator
{
    private readonly JwtOptions _jwtOptions;

    public JwtGenerator(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string GenerateUserJwt(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(CustomClaimTypes.UserId, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(CustomClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.MainEmail.Email),
            new(CustomClaimTypes.Email, user.MainEmail.Email)
        };

        var roles = user.Profiles.Select(p => p.Role.Name).Distinct();
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var permissions = user.Profiles.SelectMany(p => p.Permissions).Distinct();
        foreach (var permission in permissions)
        {
            claims.Add(new Claim(CustomClaimTypes.Permission, permission));
        }

        var privateKeyFilePath = Path.Combine(AppContext.BaseDirectory, "private_key.pem");
        var privateKeyPem = File.ReadAllText(privateKeyFilePath);

        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(privateKeyPem.ToCharArray());

        var key = new ECDsaSecurityKey(ecdsa);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256);

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