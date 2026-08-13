using FluentValidation;

namespace Tomouh.Auth.Application.Queries.CheckEmailExistence;

public class CheckEmailExistenceQueryValidator : AbstractValidator<CheckEmailExistenceQuery>
{
    public CheckEmailExistenceQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}