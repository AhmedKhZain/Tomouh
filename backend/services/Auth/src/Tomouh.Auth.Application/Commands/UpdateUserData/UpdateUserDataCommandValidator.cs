using FluentValidation;

namespace Tomouh.Auth.Application.Commands.UpdateUserData;

public class UpdateUserDataCommandValidator : AbstractValidator<UpdateUserDataCommand>
{
    public UpdateUserDataCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Idempotency key is required.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
