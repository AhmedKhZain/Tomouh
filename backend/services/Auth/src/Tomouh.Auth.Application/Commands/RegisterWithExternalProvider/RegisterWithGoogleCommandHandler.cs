using Google.Apis.Auth;
using MediatR;
using Tomouh.Auth.Application.Common;
using Tomouh.Auth.Contracts.Responses;
using Tomouh.Auth.Application.Interfaces;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Shared.Kernel.Extensions;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;
using static Tomouh.Auth.Application.Common.AuthenticationCommon;

namespace Tomouh.Auth.Application.Commands.RegisterWithExternalProvider;

public class RegisterWithExternalProviderCommandHandler : IRequestHandler<RegisterWithExternalProviderCommand, ResultOf<AuthenticationResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IExternalAuthProviderFactory _providerFactory;
    private readonly IJwtGenerator _jwtTokenGenerator;

    public RegisterWithExternalProviderCommandHandler(
        IUserRepository userRepository,
        IExternalAuthProviderFactory providerFactory,
        IJwtGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _providerFactory = providerFactory;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<ResultOf<AuthenticationResult>> Handle(RegisterWithExternalProviderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var strategy = _providerFactory.GetStrategy(request.Provider);

            var payload = await strategy.ValidateTokenAsync(request.Token, cancellationToken);

            var providerName = request.Provider.ToString();

            var existingUser = await _userRepository.GetByProviderSubjectIdAsync(providerName, payload.SubjectId, cancellationToken);
            if (existingUser is not null)
                return AuthenticationErrors.AccountAlreadyExists;

            var existingUserByEmail = await _userRepository.GetByEmailAsync(payload.Email, cancellationToken);
            if (existingUserByEmail is not null)
                return AuthenticationErrors.EmailAlreadyRegisteredWithLocalAccount;

            var user = User.CreateFromExternalProvider(
                provider: providerName,
                subjectId: payload.SubjectId,
                email: payload.Email,
                firstName: payload.FirstName,
                lastName: payload.LastName,
                profilePhotoPath: payload.PictureUrl,
                showName: payload.Name
            );

            await _userRepository.AddAsync(user, cancellationToken);

            var token = _jwtTokenGenerator.GenerateUserJwt(user);

            return new AuthenticationResult(user, token).AsDone();
        }
        catch (InvalidJwtException)
        {
            return AuthenticationErrors.InvalidExternalToken;
        }
        catch (NotSupportedException)
        {
            return AuthenticationErrors.UnsupportedExternalProvider;
        }
        catch (HttpRequestException)
        {
            return AuthenticationErrors.ExternalAuthFailed;
        }
        catch (Exception ex)
        {
            return Error.Failure(
                code: "RegisterWithExternalProviderCommandHandler",
                description: $"{ex.Message}");
        }
    }
}