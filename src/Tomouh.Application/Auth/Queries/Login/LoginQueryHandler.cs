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

namespace Tomouh.Application.Auth.Queries.Login;

public class LoginQueryHandler(
    IJwtGenerator _tokenGenerator,
    IPasswordHasher _passwordHasher,
    ITokenHasher _tokenHasher,
    ICacheService<User> _cacheService,
    IUserTokenRepository _tokenRepository,
    IHttpContextAccessor _contextAccessor)
        : IRequestHandler<LoginQuery, ResultOf<AuthenticationResult>>
{
    public async Task<ResultOf<AuthenticationResult>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _cacheService.GetAsync(cacheKey: UserOptimisticLoadingCachePrefix + query.Email);

            if (user is null)
                return AuthenticationErrors.SomethingGoesWrongEnterEmailAgain;

            var passwordCheckResult = user.IsCorrectPasswordHash(query.Password, _passwordHasher);

            if (passwordCheckResult.IsFailure)
                return passwordCheckResult.Errors;

            var isPasswordCorrect = passwordCheckResult.Value;

            if (!isPasswordCorrect)
                return AuthenticationErrors.InvalidCredentials;

            if (user.TFA.IsTFAEnabled)
            {
                var tfaCreateResult = user.GenerateToken(TokenType.TwoFactorAuthentication, _tokenHasher, out var tfaToken);
                if (tfaCreateResult.IsFailure)
                    return tfaCreateResult.Errors;

                return ((AuthenticationResult)
                    new TFANeededAuthenticationResult(user)).AsPartial();
            }

            var token = _tokenGenerator.GenerateUserJwtToken(user);

            var refreshTokenCreationResult = user.GenerateToken(TokenType.RefreshToken, _tokenHasher, out var refreshToken);

            if (refreshTokenCreationResult.IsFailure)
                return refreshTokenCreationResult.Errors;

            var refreshTokenEntity = refreshTokenCreationResult.Value;
            await _tokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

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

            _contextAccessor.HttpContext?.Response.Cookies.Append(AccessTokenCookieName, token, accessTokenCookieOptions);
            _contextAccessor.HttpContext?.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshTokenCookieOptions);

            return ((AuthenticationResult)
                new FullAuthenticationResult(user, token, refreshToken)).AsDone();
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "LoginQueryHandler",
                description: ex.Message
            );
        }
    }
}