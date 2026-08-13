using MediatR;
using Microsoft.AspNetCore.Http;
using Tomouh.Auth.Application.Common;
using Tomouh.Auth.Application.Interfaces;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Extensions;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Queries.RefreshToken;

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

            var newAccessToken = _tokenGenerator.GenerateUserJwt(user);

            var newRefreshTokenResult = user.GenerateToken(TokenType.RefreshToken, _tokenHasher, out var newRefreshToken);
            if (newRefreshTokenResult.IsFailure)
                return newRefreshTokenResult.Errors;

            var newTokenEntity = newRefreshTokenResult.Value;
            await _tokenRepository.AddAsync(newTokenEntity, cancellationToken);


            var refreshTokenCookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.Add(RefreshTokenCookieExpiration),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(RefreshTokenCookieName, newRefreshToken, refreshTokenCookieOptions);

            return ((AuthenticationResult)
                new FullAuthenticationResult(user, newAccessToken, DateTime.UtcNow.AddMinutes(30), newRefreshToken)).AsDone();
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