using MediatR;
using Microsoft.AspNetCore.Http;
using Tomouh.Auth.Application.Common;
using Tomouh.Auth.Application.Interfaces;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Extensions;
using Tomouh.Shared.Kernel.Features;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Queries.Login;

public class LoginQueryHandler(
    IJwtGenerator _tokenGenerator,
    IPasswordHasher _passwordHasher,
    ITokenHasher _tokenHasher,
    ICacheService _cacheService,
    IUserTokenRepository _tokenRepository,
    IHttpContextAccessor _contextAccessor)
        : IRequestHandler<LoginQuery, ResultOf<AuthenticationResult>>
{
    public async Task<ResultOf<AuthenticationResult>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _cacheService.GetAsync<User>(cacheKey: UserOptimisticLoadingCachePrefix + query.Email);

            if (user is null)
                return AuthenticationErrors.SomethingGoesWrongEnterEmailAgain;

            var passwordCheckResult = user.IsCorrectPasswordHash(query.Password, _passwordHasher);

            if (passwordCheckResult.IsFailure)
                return passwordCheckResult.Errors!;

            var isPasswordCorrect = passwordCheckResult.Value;

            if (!isPasswordCorrect)
                return AuthenticationErrors.InvalidCredentials;

            if (user.TFA.IsTFAEnabled)
            {
                var tfaCreateResult = user.GenerateToken(TokenType.TwoFactorAuthentication, _tokenHasher, out var tfaToken);
                if (tfaCreateResult.IsFailure)
                    return tfaCreateResult.Errors!;

                return ((AuthenticationResult)
                    new TFANeededAuthenticationResult(user)).AsPartial();
            }

            var token = _tokenGenerator.GenerateUserJwt(user);

            var refreshTokenCreationResult = user.GenerateToken(TokenType.RefreshToken, _tokenHasher, out var refreshToken);

            if (refreshTokenCreationResult.IsFailure)
                return refreshTokenCreationResult.Errors!;

            var refreshTokenEntity = refreshTokenCreationResult.Value;
            await _tokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

            var refreshTokenCookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.Add(RefreshTokenCookieExpiration),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            _contextAccessor.HttpContext?.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshTokenCookieOptions);

            return ((AuthenticationResult)
                new FullAuthenticationResult(user, token, DateTime.UtcNow.AddMinutes(30), refreshToken)).AsDone();
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