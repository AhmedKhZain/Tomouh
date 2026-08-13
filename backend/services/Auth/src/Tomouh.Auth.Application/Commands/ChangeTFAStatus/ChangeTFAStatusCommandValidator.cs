using FluentValidation;

namespace Tomouh.Auth.Application.Commands.ChangeTFAStatus;

public class ChangeTFAStatusCommandValidator : AbstractValidator<ChangeTFAStatusCommand>
{
    public ChangeTFAStatusCommandValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Idempotency key is required.");
    }
}
