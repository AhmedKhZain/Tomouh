using Common.Errors;
using Common.Extinctions;
using Common.ResultOf;
using Common.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Tomouh.Application.Auth.Common;
using Tomouh.Application.Common.Interfaces;
using Tomouh.Domain.Auth;
using static Tomouh.Application.Auth.AuthenticationCommon;

namespace Tomouh.Application.Auth.Queries.Login;

public class LoginQueryHandler(
    ITokenGenerator _tokenGenerator,
    IPasswordHasher _passwordHasher,
    ITokenHasher _tokenHasher,
    ICacheService<User> _cacheService,
    IHttpContextAccessor _contextAccessor)
        : IRequestHandler<LoginQuery, ResultOf<AuthenticationResultBase>>
{
    public async Task<ResultOf<AuthenticationResultBase>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _cacheService.GetAsync(cacheKey: UserOptimisticLoadingCachePrefix + query.Email);


            if (user is null)
                return AuthenticationErrors.SomethingGoseWrongEnterEmailAgain;

            var PasswordCheckResult = user.IsCorrectPasswordHash(query.Password, _passwordHasher);

            if (PasswordCheckResult.IsFailure)
                return PasswordCheckResult.Errors;

            var isPasswordCorrect = PasswordCheckResult.Value;

            if (!isPasswordCorrect)
                return AuthenticationErrors.InvalidCredentials;


            if (user.TFA.IsTFAEnabled)
            {
                var tfaCreateResult = user.GenerateToken(TokenType.TwoFactorAuthentication, _tokenHasher, out var tfaToken);
                if (tfaCreateResult.IsFailure)
                    return tfaCreateResult.Errors;

                return ((AuthenticationResultBase)
                    new TFANeededAuthenticationResult(user)).AsPartial();
            }


            var token = _tokenGenerator.GenerateUserJwtToken(user);

            var refreshTokenCreationResult = user.GenerateToken(TokenType.RefreshToken, _tokenHasher, out var refreshToken);


            if (refreshTokenCreationResult.IsFailure)
                return refreshTokenCreationResult.Errors;

            _contextAccessor.HttpContext.Response.Cookies.Append(AccessTokenCookieName, token);
            _contextAccessor.HttpContext.Response.Cookies.Append(RefreshTokenCookieName, refreshToken);

            return ((AuthenticationResultBase)
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