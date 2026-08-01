using Common.Extinctions;
using Common.ResultOf;
using Common.ResultOf.Errors;
using Common.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Tomouh.Application.Auth.Common;
using Tomouh.Application.Common.Interfaces.Services;
using Tomouh.Domain.Auth;
using Tomouh.Domain.Auth.Repositories;
using static Tomouh.Application.Auth.Common.AuthenticationCommon;

namespace Tomouh.Application.Auth.Commands.Register;

public class RegisterUserCommandHandler(
    IUserRepository _userRepository,
    IUserTokenRepository _tokenRepository,
    IPasswordHasher _passwordHasher,
    IJwtGenerator _tokenGenerator,
    ITokenHasher _tokenHasher,
    IHttpContextAccessor _httpContextAccessor)
    : IRequestHandler<RegisterUserCommand, ResultOf<AuthenticationResult>>
{
    public async Task<ResultOf<AuthenticationResult>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (exists != null)
            {
                exists.MarkEmailFound();
                return new AuthenticationResult(exists, "User Exists with the same Email try login.");
            }

            var userToAdd = User.CreateLocal(request.ShowName, request.FirstName, request.LastName, request.Email, request.Password, _passwordHasher);

            if (userToAdd.IsFailure)
                return userToAdd.Errors;

            var user = userToAdd.Value;
            await _userRepository.AddAsync(user, cancellationToken);

            var accessToken = _tokenGenerator.GenerateUserJwtToken(user);

            var refreshTokenResult = user.GenerateToken(TokenType.RefreshToken, _tokenHasher, out var refreshToken);

            if (refreshTokenResult.IsFailure)
                return refreshTokenResult.Errors;

            var tokenEntity = refreshTokenResult.Value;
            await _tokenRepository.AddAsync(tokenEntity, cancellationToken);

            var accessTokenCookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.Add(AccessTokenCookieExpiration),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            var refreshTokenCookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.Add(RefreshTokenCookieExpiration),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(AccessTokenCookieName, accessToken, accessTokenCookieOptions);
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshTokenCookieOptions);

            return ((AuthenticationResult)
                new FullAuthenticationResult(user, accessToken, refreshToken)).AsDone();
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "RegisterUserCommandHandler",
                description: ex.Message
            );
        }
    }
}