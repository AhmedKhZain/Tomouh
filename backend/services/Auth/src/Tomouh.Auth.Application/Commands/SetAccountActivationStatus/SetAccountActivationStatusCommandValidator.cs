using FluentValidation;

namespace Tomouh.Auth.Application.Commands.SetAccountActivationStatus;

public class SetAccountActivationStatusCommandValidator : AbstractValidator<SetAccountActivationStatusCommand>
{
    public SetAccountActivationStatusCommandValidator()
    {
        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user id is required.");

        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Idempotency key is required.");
    }
}
