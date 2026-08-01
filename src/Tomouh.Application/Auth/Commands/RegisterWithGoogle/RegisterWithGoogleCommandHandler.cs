using Common.Extinctions;
using Common.ResultOf;
using Common.ResultOf.Errors;
using Google.Apis.Auth;
using MediatR;
using Tomouh.Application.Auth.Common;
using Tomouh.Application.Common.Interfaces;
using Tomouh.Application.Common.Interfaces.Services;
using Tomouh.Domain.Auth;
using Tomouh.Domain.Auth.Repositories;
using static Tomouh.Application.Auth.Common.AuthenticationCommon;

namespace Tomouh.Application.Auth.Commands.RegisterWithGoogle;

public class RegisterWithGoogleCommandHandler(
    IUserRepository _userRepository,
    IGoogleAuthService _googleAuthService,
    IJwtGenerator _jwtTokenGenerator
    ) : IRequestHandler<RegisterWithGoogleCommand, ResultOf<AuthenticationResult>>
{
    public async Task<ResultOf<AuthenticationResult>> Handle(RegisterWithGoogleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await _googleAuthService.ValidateTokenAsync(request.GoogleToken, cancellationToken);

            var existingUser = await _userRepository.GetByProviderSubjectIdAsync("Google", payload.SubjectId, cancellationToken);
            if (existingUser is not null)
                return AuthenticationErrors.AccountAlreadyExists;

            var existingUserByEmail = await _userRepository.GetByEmailAsync(payload.Email, cancellationToken);
            if (existingUserByEmail is not null)
                return AuthenticationErrors.EmailAlreadyRegisteredWithLocalAccount;

            var user = User.CreateFromGoogle(
                googleSubjectId: payload.SubjectId,
                email: payload.Email,
                firstName: payload.FirstName,
                lastName: payload.LastName,
                profilePhotoPath: payload.PictureUrl,
                showName: payload.Name
            );

            await _userRepository.AddAsync(user, cancellationToken);

            var token = _jwtTokenGenerator.GenerateUserJwtToken(user);

            return new AuthenticationResult(user, token).AsDone();
        }
        catch (InvalidJwtException)
        {
            return AuthenticationErrors.InvalidGoogleToken;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "RegisterWithGoogleCommandHandler",
                description: $"{ex.Message}");
        }
    }
}