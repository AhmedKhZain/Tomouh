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

namespace Tomouh.Application.Auth.Queries.RefreshToken;

public class RefreshTokenQueryHandler(
    IUserRepository _userRepository,
    IUserTokenRepository _tokenRepository,
    IJwtGenerator _tokenGenerator,
    ITokenHasher _tokenHasher,
    IHttpContextAccessor _httpContextAccessor)
        : IRequestHandler<RefreshTokenQuery, ResultOf<AuthenticationResult>>
{
    public async Task<ResultOf<AuthenticationResult>> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies[RefreshTokenCookieName];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return AuthenticationErrors.RefreshTokenMissing;
            }

            var hashResult = _tokenHasher.Hash(refreshToken);
            if (hashResult.IsFailure)
                return hashResult.Errors;

            var tokenEntity = await _tokenRepository.GetAsync(
                tokenHash: hashResult.Value,
                tokenType: TokenType.RefreshToken,
                isUsed: false,
                isRevoked: false,
                includeExpired: false,
                cancellationToken: cancellationToken);

            if (tokenEntity is null)
            {
                return AuthenticationErrors.InvalidRefreshToken;
            }

            var tokenUseResult = tokenEntity.MarkUsed(refreshToken, _tokenHasher);

            tokenEntity.Revoke(TokenRevokeCause.Used);

            if (tokenUseResult.IsFailure)
                return tokenUseResult.Errors;

            var user = await _userRepository.GetByIdAsync(tokenEntity.UserId, cancellationToken);
            if (user is null)
            {
                return AuthenticationErrors.UserNotFound;
            }

            await _tokenRepository.UpdateAsync(tokenEntity, cancellationToken);

            var newAccessToken = _tokenGenerator.GenerateUserJwtToken(user);

            var newRefreshTokenResult = user.GenerateToken(TokenType.RefreshToken, _tokenHasher, out var newRefreshToken);
            if (newRefreshTokenResult.IsFailure)
                return newRefreshTokenResult.Errors;

            var newTokenEntity = newRefreshTokenResult.Value;
            await _tokenRepository.AddAsync(newTokenEntity, cancellationToken);

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

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(AccessTokenCookieName, newAccessToken, accessTokenCookieOptions);
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(RefreshTokenCookieName, newRefreshToken, refreshTokenCookieOptions);

            return ((AuthenticationResult)
                new FullAuthenticationResult(user, newAccessToken, newRefreshToken)).AsDone();
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "RefreshTokenQueryHandler",
                description: ex.Message
            );
        }
    }
}