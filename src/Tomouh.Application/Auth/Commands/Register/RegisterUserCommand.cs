using Common.Errors;
using Common.Requests;
using Common.ResultOf;
using Common.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Tomouh.Application.Auth.Common;
using Tomouh.Domain.Auth;
using Tomouh.Domain.Auth.Repositories;

namespace Tomouh.Application.Auth.Commands.Register;

public record RegisterUserCommand(
    string ShowName,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    Guid RequestId)
    : ICommand<ResultOf<AuthenticationResultBase>>,
    IIdempotentRequest,
    IEventsIncludedRequest;
public class RegisterUserCommandHandler(
    IUserRepository _userRepository,
    IPasswordHasher _passwordHasher,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<RegisterUserCommand, ResultOf<AuthenticationResultBase>>
{
    public async Task<ResultOf<AuthenticationResultBase>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var exites = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (exites != null)
            {
                exites.MarkEmailFound();
                return new AuthenticationResultBase(exites, "User Existes with the same Email try login.");
            }
            var userToAdd = new User(request.ShowName, request.FirstName, request.LastName, request.Email);

            var hashingUseerPasswordResult = userToAdd.SetNewPassword(request.Password, _passwordHasher);
            if (hashingUseerPasswordResult.IsFailure)
                return hashingUseerPasswordResult.Errors;




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