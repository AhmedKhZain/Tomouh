using FluentValidation;

namespace Tomouh.Application.Auth.Queries.CheckEmailExistence;

public class CheckEmailExistenceQueryValidator : AbstractValidator<CheckEmailExistenceQuery>
{
    public CheckEmailExistenceQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}