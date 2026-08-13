using FluentValidation;
using Tomouh.Auth.Application.Queries.Login;

namespace Tomouh.Auth.Application.Queries.Login;

public class LogInQueryValidator : AbstractValidator<LoginQuery>
{
    public LogInQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(80);
    }
}