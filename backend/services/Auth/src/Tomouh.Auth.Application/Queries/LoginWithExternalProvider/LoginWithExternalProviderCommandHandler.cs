using Google.Apis.Auth;
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

namespace Tomouh.Auth.Application.Queries.LoginWithExternalProvider;

public sealed class LoginWithExternalProviderCommandHandler(
    IUserRepository _userRepository,
    IExternalAuthProviderFactory _providerFactory,
    IJwtGenerator _jwtTokenGenerator,
    ITokenHasher _tokenHasher,
    IUserTokenRepository _tokenRepository,
    IHttpContextAccessor _contextAccessor)
    : IRequestHandler<LoginWithExternalProviderQuery, ResultOf<AuthenticationResult>>
{
    public async Task<ResultOf<AuthenticationResult>> Handle(
        LoginWithExternalProviderQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var strategy = _providerFactory.GetStrategy(request.Provider);
            var payload = await strategy.ValidateTokenAsync(request.Token, cancellationToken);

            var user = await _userRepository.GetByProviderSubjectIdAsync(
                request.Provider.ToString(),
                payload.SubjectId,
                cancellationToken);

            if (user is null)
            {
                return AuthenticationErrors.ExternalLoginNotFound;
            }

            // Check if Two-Factor Authentication (2FA) is enabled
            if (user.TFA.IsTFAEnabled)
            {
                var tfaCreateResult = user.GenerateToken(TokenType.TwoFactorAuthentication, _tokenHasher, out var tfaToken);
                if (tfaCreateResult.IsFailure)
                    return tfaCreateResult.Errors!;

                return ((AuthenticationResult)
                    new TFANeededAuthenticationResult(user)).AsPartial();
            }

            // Generate Access Token
            var token = _jwtTokenGenerator.GenerateUserJwt(user);

            // Generate & persist Refresh Token
            var refreshTokenCreationResult = user.GenerateToken(TokenType.RefreshToken, _tokenHasher, out var refreshToken);
            if (refreshTokenCreationResult.IsFailure)
                return refreshTokenCreationResult.Errors!;

            var refreshTokenEntity = refreshTokenCreationResult.Value;
            await _tokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

            // Append Refresh Token to Cookie
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
        catch (InvalidJwtException)
        {
            return AuthenticationErrors.InvalidExternalToken;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "LoginWithExternalProviderCommandHandler",
                description: ex.Message
            );
        }
    }
}